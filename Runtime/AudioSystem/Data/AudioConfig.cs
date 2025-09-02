using UnityEngine;
using System.Collections.Generic;

namespace AudioEffectKit
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] private List<AudioClipData> _audioClips = new List<AudioClipData>();
        [SerializeField] private Dictionary<AudioCategory, float> _defaultCategoryVolumes = new Dictionary<AudioCategory, float>();
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private int _maxConcurrentSounds = 32;
        [SerializeField] private bool _enablePoolPreWarm = true;
        [SerializeField] private int _poolPreWarmSize = 10;
        [SerializeField] private bool _enable3DAudio = true;
        [SerializeField] private AudioRolloffMode _defaultRolloffMode = AudioRolloffMode.Logarithmic;

        public List<AudioClipData> AudioClips => _audioClips;
        public Dictionary<AudioCategory, float> DefaultCategoryVolumes => _defaultCategoryVolumes;
        public float MasterVolume => _masterVolume;
        public int MaxConcurrentSounds => _maxConcurrentSounds;
        public bool EnablePoolPreWarm => _enablePoolPreWarm;
        public int PoolPreWarmSize => _poolPreWarmSize;
        public bool Enable3DAudio => _enable3DAudio;
        public AudioRolloffMode DefaultRolloffMode => _defaultRolloffMode;

        public AudioClipData GetAudioClip(string name)
        {
            return _audioClips.Find(clip => clip.Name == name);
        }

        public List<AudioClipData> GetAudioClipsByCategory(AudioCategory category)
        {
            return _audioClips.FindAll(clip => clip.Category == category);
        }

        public void AddAudioClip(AudioClipData clipData)
        {
            if (clipData != null && !ContainsAudioClip(clipData.Name))
            {
                _audioClips.Add(clipData);
            }
        }

        public void RemoveAudioClip(string name)
        {
            var clipData = GetAudioClip(name);
            if (clipData != null)
            {
                _audioClips.Remove(clipData);
            }
        }

        public bool ContainsAudioClip(string name)
        {
            return _audioClips.Exists(clip => clip.Name == name);
        }

        public List<string> GetAllSoundNames()
        {
            var names = new List<string>();
            foreach (var clip in _audioClips)
            {
                names.Add(clip.Name);
            }
            return names;
        }

        public bool ValidateConfig()
        {
            return ValidateUniqueNames() && _audioClips.TrueForAll(clip => clip.IsValid());
        }

        public static AudioConfig CreateDefault()
        {
            var config = CreateInstance<AudioConfig>();
            config.InitializeDefaultVolumes();
            return config;
        }

        private void OnValidate()
        {
            _masterVolume = Mathf.Clamp01(_masterVolume);
            _maxConcurrentSounds = Mathf.Max(1, _maxConcurrentSounds);
            _poolPreWarmSize = Mathf.Max(1, _poolPreWarmSize);
            
            if (_defaultCategoryVolumes.Count == 0)
            {
                InitializeDefaultVolumes();
            }
        }

        private void InitializeDefaultVolumes()
        {
            _defaultCategoryVolumes.Clear();
            _defaultCategoryVolumes[AudioCategory.SFX] = 1.0f;
            _defaultCategoryVolumes[AudioCategory.Music] = 0.8f;
            _defaultCategoryVolumes[AudioCategory.Voice] = 1.0f;
            _defaultCategoryVolumes[AudioCategory.UI] = 0.9f;
            _defaultCategoryVolumes[AudioCategory.Ambient] = 0.7f;
            _defaultCategoryVolumes[AudioCategory.Combat] = 1.0f;
        }

        private bool ValidateUniqueNames()
        {
            var names = new HashSet<string>();
            foreach (var clip in _audioClips)
            {
                if (!string.IsNullOrEmpty(clip.Name))
                {
                    if (names.Contains(clip.Name))
                    {
                        return false;
                    }
                    names.Add(clip.Name);
                }
            }
            return true;
        }
    }
}