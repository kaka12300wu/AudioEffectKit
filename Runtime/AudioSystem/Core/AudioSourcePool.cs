using UnityEngine;
using System.Collections.Generic;
using Core.Runtime.PoolKit;

namespace AudioEffectKit
{
    public class AudioSourcePool
    {
        private ComponentPool<AudioSource> _pool;
        private AudioSource _audioSourcePrefab;
        private Transform _poolParent;
        private int _initialPoolSize = 10;
        private int _maxPoolSize = 50;
        private List<AudioSource> _activeSources = new List<AudioSource>();

        public int PoolSize => _initialPoolSize;
        public int ActiveCount => _activeSources.Count;
        public int TotalCount => PoolSize + ActiveCount;
        public bool IsInitialized { get; private set; }

        public void Initialize(Transform parent, int initialSize = 10)
        {
            _poolParent = parent;
            _initialPoolSize = initialSize;
            
            _pool = new ComponentPool<AudioSource>(_audioSourcePrefab, _poolParent);
            
            PreWarm(_initialPoolSize);
            IsInitialized = true;
        }

        public AudioSource Get()
        {
            if (!IsInitialized)
            {
                Debug.LogError("AudioSourcePool is not initialized!");
                return null;
            }

            var audioSource = _pool.Get();
            if (audioSource != null)
            {
                _activeSources.Add(audioSource);
                ResetAudioSource(audioSource);
                SetupAudioSource(audioSource);
                audioSource.gameObject.SetActive(true);
            }
            
            return audioSource;
        }

        public void Return(AudioSource audioSource)
        {
            if (audioSource == null || !IsInitialized) return;

            _activeSources.Remove(audioSource);
            ResetAudioSource(audioSource);
            audioSource.gameObject.SetActive(false);
            
            _pool.Return(audioSource);
            CheckPoolCapacity();
        }

        public void PreWarm(int count)
        {
            if (!IsInitialized) return;

            for (int i = 0; i < count; i++)
            {
                var source = _pool.Get();
                if (source != null)
                {
                    ResetAudioSource(source);
                    _pool.Return(source);
                }
            }
        }

        public void Clear()
        {
            if (!IsInitialized) return;

            _activeSources.Clear();
            _pool.Clear();
            
            if (_audioSourcePrefab != null)
            {
                Object.DestroyImmediate(_audioSourcePrefab);
                _audioSourcePrefab = null;
            }
            
            IsInitialized = false;
        }

        public void SetMaxPoolSize(int maxSize)
        {
            _maxPoolSize = Mathf.Max(1, maxSize);
        }

        public List<AudioSource> GetActiveSources()
        {
            return new List<AudioSource>(_activeSources);
        }

        private GameObject CreateAudioSourcePrefab()
        {
            var prefab = new GameObject("AudioSource");
            prefab.AddComponent<AudioSource>();
            prefab.SetActive(false);
            return prefab;
        }

        private void SetupAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.pitch = 1f;
            source.loop = false;
            source.priority = 128;
        }

        private void ResetAudioSource(AudioSource source)
        {
            if (source == null) return;

            source.Stop();
            source.clip = null;
            source.volume = 1f;
            source.pitch = 1f;
            source.loop = false;
            source.spatialBlend = 0f;
            source.priority = 128;
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 1f;
            source.maxDistance = 500f;
            source.time = 0f;
        }

        private void CheckPoolCapacity()
        {
            if (PoolSize > _maxPoolSize)
            {
                int excess = PoolSize - _maxPoolSize;
                for (int i = 0; i < excess; i++)
                {
                    var source = _pool.Get();
                    if (source != null)
                    {
                        Object.DestroyImmediate(source.gameObject);
                    }
                }
            }
        }
    }
}