using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AudioEffectKit
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private Dictionary<AudioCategory, float> _categoryVolumes = new Dictionary<AudioCategory, float>();
        private Dictionary<string, AudioClipData> _audioDatabase = new Dictionary<string, AudioClipData>();
        private AudioSourcePool _audioSourcePool;
        private Transform _audioRoot;
        private List<AudioHandle> _activeHandles = new List<AudioHandle>();
        private List<IAudioEffect> _audioEffects = new List<IAudioEffect>();
        private bool _isInitialized = false;
        private float _masterVolume = 1.0f;
        private bool _isMuted = false;
        private Coroutine _fadeCoroutine;
        private int _maxConcurrentSounds = 32;

        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateAllCategoryVolumes();
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                UpdateAllCategoryVolumes();
            }
        }

        public bool IsInitialized => _isInitialized;
        public int ActiveSoundCount => _activeHandles.Count(h => h.IsValid && h.IsPlaying);

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            if (!_isInitialized) return;

            RemoveInvalidHandles();
            
            foreach (var handle in _activeHandles.ToList())
            {
                handle.Update();
            }

            foreach (var effect in _audioEffects)
            {
                var activeSources = _audioSourcePool.GetActiveSources();
                foreach (var source in activeSources)
                {
                    effect.OnAudioUpdate(source, Time.deltaTime);
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                StopAllSounds();
                _audioSourcePool?.Clear();
                _activeHandles.Clear();
                _audioEffects.Clear();
                _isInitialized = false;
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            _audioRoot = new GameObject("AudioRoot").transform;
            _audioRoot.SetParent(transform);
            
            _audioSourcePool = new AudioSourcePool();
            _audioSourcePool.Initialize(_audioRoot);
            
            InitializeDefaultCategoryVolumes();
            _isInitialized = true;
        }

        public void LoadAudioDatabase(AudioConfig config)
        {
            if (config == null) return;

            _audioDatabase.Clear();
            foreach (var clipData in config.AudioClips)
            {
                if (clipData != null && !string.IsNullOrEmpty(clipData.Name))
                {
                    _audioDatabase[clipData.Name] = clipData;
                }
            }

            _masterVolume = config.MasterVolume;
            _maxConcurrentSounds = config.MaxConcurrentSounds;
            
            foreach (var kvp in config.DefaultCategoryVolumes)
            {
                _categoryVolumes[kvp.Key] = kvp.Value;
            }

            if (config.EnablePoolPreWarm)
            {
                _audioSourcePool.PreWarm(config.PoolPreWarmSize);
            }
        }

        public AudioHandle PlaySound(string soundName, Vector3 position = default)
        {
            if (!CanPlayNewSound()) return null;

            var clipData = GetSoundData(soundName);
            if (clipData == null) return null;

            return PlaySound(clipData, position);
        }

        public AudioHandle PlaySound(AudioClipData clipData, Vector3 position = default)
        {
            if (clipData == null || !CanPlayNewSound()) return null;

            var audioSource = GetAudioSource();
            if (audioSource == null) return null;

            audioSource.transform.position = position;
            ApplyAudioSettings(audioSource, clipData);
            ApplyAudioEffects(audioSource, clipData);

            var handle = CreateHandle(audioSource, clipData);
            audioSource.Play();

            return handle;
        }

        public AudioHandle PlayMusic(string musicName, bool loop = true)
        {
            var clipData = GetSoundData(musicName);
            if (clipData == null) return null;

            var modifiedData = clipData.Clone();
            // Set loop through reflection since the field might be private
            var loopField = typeof(AudioClipData).GetField("_loop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loopField?.SetValue(modifiedData, loop);

            return PlaySound(modifiedData);
        }

        public void PlayOneShot(string soundName, Vector3 position = default)
        {
            var handle = PlaySound(soundName, position);
            if (handle != null)
            {
                handle.SetOnCompleteCallback(() => handle.Stop());
            }
        }

        public void StopSound(AudioHandle handle)
        {
            if (handle != null && handle.IsValid)
            {
                handle.Stop();
            }
        }

        public void StopAllSounds()
        {
            foreach (var handle in _activeHandles.ToList())
            {
                if (handle.IsValid)
                {
                    handle.Stop();
                }
            }
            _activeHandles.Clear();
        }

        public void StopCategory(AudioCategory category)
        {
            var handlesCopy = _activeHandles.Where(h => h.IsValid && h.Category == category).ToList();
            foreach (var handle in handlesCopy)
            {
                handle.Stop();
            }
        }

        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            _categoryVolumes[category] = Mathf.Clamp01(volume);
            UpdateCategoryVolume(category);
        }

        public float GetCategoryVolume(AudioCategory category)
        {
            return _categoryVolumes.TryGetValue(category, out var volume) ? volume : 1.0f;
        }

        public void PauseCategory(AudioCategory category)
        {
            foreach (var handle in _activeHandles.Where(h => h.IsValid && h.Category == category))
            {
                handle.Pause();
            }
        }

        public void ResumeCategory(AudioCategory category)
        {
            foreach (var handle in _activeHandles.Where(h => h.IsValid && h.Category == category))
            {
                handle.Resume();
            }
        }

        public void RegisterAudioEffect<T>() where T : IAudioEffect, new()
        {
            var effect = new T();
            if (_audioEffects.Any(e => e.GetType() == typeof(T))) return;

            _audioEffects.Add(effect);
            _audioEffects.Sort((a, b) => a.GetPriority().CompareTo(b.GetPriority()));
        }

        public void UnregisterAudioEffect<T>() where T : IAudioEffect
        {
            _audioEffects.RemoveAll(e => e.GetType() == typeof(T));
        }

        public void FadeInMasterVolume(float targetVolume, float duration)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(MasterVolumeFadeCoroutine(targetVolume, duration));
        }

        public void FadeOutMasterVolume(float targetVolume, float duration)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(MasterVolumeFadeCoroutine(targetVolume, duration));
        }

        public bool HasSound(string soundName)
        {
            return _audioDatabase.ContainsKey(soundName);
        }

        public AudioClipData GetSoundData(string soundName)
        {
            _audioDatabase.TryGetValue(soundName, out var clipData);
            return clipData;
        }

        internal void ReturnAudioSourceToPool(AudioSource source)
        {
            _audioSourcePool?.Return(source);
        }

        private AudioSource GetAudioSource()
        {
            return _audioSourcePool?.Get();
        }

        private void ApplyAudioSettings(AudioSource source, AudioClipData clipData)
        {
            clipData.ApplyToAudioSource(source);
        }

        private void UpdateCategoryVolume(AudioCategory category)
        {
            foreach (var handle in _activeHandles.Where(h => h.IsValid && h.Category == category))
            {
                handle.ApplyVolumeSettings();
            }
        }

        private void UpdateAllCategoryVolumes()
        {
            foreach (var category in System.Enum.GetValues(typeof(AudioCategory)).Cast<AudioCategory>())
            {
                UpdateCategoryVolume(category);
            }
        }

        private AudioHandle CreateHandle(AudioSource source, AudioClipData clipData)
        {
            var handle = new AudioHandle();
            handle.Initialize(source, this, clipData.Name, clipData.Category);
            _activeHandles.Add(handle);
            return handle;
        }

        private void RemoveInvalidHandles()
        {
            _activeHandles.RemoveAll(h => !h.IsValid);
        }

        private void ApplyAudioEffects(AudioSource source, AudioClipData clipData)
        {
            foreach (var effect in _audioEffects)
            {
                if (effect.CanProcess(clipData.Category))
                {
                    effect.OnAudioPlay(source, clipData);
                }
            }
        }

        private IEnumerator MasterVolumeFadeCoroutine(float targetVolume, float duration)
        {
            float startVolume = _masterVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                MasterVolume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }

            MasterVolume = targetVolume;
            _fadeCoroutine = null;
        }

        private bool CanPlayNewSound()
        {
            if (!_isInitialized) return false;
            
            int activeSounds = _activeHandles.Count(h => h.IsValid && h.IsPlaying);
            return activeSounds < _maxConcurrentSounds;
        }

        private void InitializeDefaultCategoryVolumes()
        {
            foreach (AudioCategory category in System.Enum.GetValues(typeof(AudioCategory)))
            {
                if (!_categoryVolumes.ContainsKey(category))
                {
                    _categoryVolumes[category] = 1.0f;
                }
            }
        }
    }
}