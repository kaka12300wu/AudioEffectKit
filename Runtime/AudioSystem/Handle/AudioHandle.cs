using UnityEngine;
using System;
using System.Collections;

namespace AudioEffectKit
{
    public class AudioHandle
    {
        private AudioSource _audioSource;
        private AudioManager _manager;
        private bool _isValid;
        private string _soundName;
        private AudioCategory _category;
        private float _originalVolume;
        private bool _isPaused;
        private Coroutine _fadeCoroutine;
        private System.Action _onComplete;
        private float _playStartTime;

        public bool IsValid => _isValid && _audioSource != null;
        public bool IsPlaying => IsValid && _audioSource.isPlaying;
        public bool IsPaused => _isPaused;
        
        public float Volume
        {
            get => IsValid ? _originalVolume : 0f;
            set
            {
                if (IsValid)
                {
                    _originalVolume = Mathf.Clamp01(value);
                    ApplyVolumeSettings();
                }
            }
        }
        
        public float Pitch
        {
            get => IsValid ? _audioSource.pitch : 1f;
            set
            {
                if (IsValid)
                {
                    _audioSource.pitch = Mathf.Clamp(value, 0.1f, 3f);
                }
            }
        }
        
        public Vector3 Position
        {
            get => IsValid ? _audioSource.transform.position : Vector3.zero;
            set
            {
                if (IsValid)
                {
                    _audioSource.transform.position = value;
                }
            }
        }
        
        public string SoundName => _soundName;
        public AudioCategory Category => _category;
        
        public float Progress
        {
            get
            {
                if (!IsValid || _audioSource.clip == null) return 0f;
                return _audioSource.time / _audioSource.clip.length;
            }
            set
            {
                if (IsValid && _audioSource.clip != null)
                {
                    _audioSource.time = Mathf.Clamp01(value) * _audioSource.clip.length;
                }
            }
        }
        
        public float Duration => IsValid && _audioSource.clip != null ? _audioSource.clip.length : 0f;
        public float PlayTime => IsValid ? Time.time - _playStartTime : 0f;
        public AudioSource AudioSource => _audioSource;

        internal void Initialize(AudioSource source, AudioManager manager, string name, AudioCategory category)
        {
            _audioSource = source;
            _manager = manager;
            _soundName = name;
            _category = category;
            _originalVolume = source.volume;
            _isValid = true;
            _isPaused = false;
            _playStartTime = Time.time;
        }

        public void Stop()
        {
            if (!IsValid) return;

            if (_fadeCoroutine != null)
            {
                _manager.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _audioSource.Stop();
            _manager?.ReturnAudioSourceToPool(_audioSource);
            Invalidate();
        }

        public void Pause()
        {
            if (!IsValid || _isPaused) return;

            _audioSource.Pause();
            _isPaused = true;
        }

        public void Resume()
        {
            if (!IsValid || !_isPaused) return;

            _audioSource.UnPause();
            _isPaused = false;
        }

        public void FadeOut(float duration)
        {
            if (!IsValid) return;

            if (_fadeCoroutine != null)
            {
                _manager.StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = _manager.StartCoroutine(FadeCoroutine(0f, duration));
        }

        public void FadeIn(float duration)
        {
            if (!IsValid) return;

            if (_fadeCoroutine != null)
            {
                _manager.StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = _manager.StartCoroutine(FadeCoroutine(_originalVolume, duration));
        }

        public void SetProgress(float progress)
        {
            Progress = progress;
        }

        public float GetProgress()
        {
            return Progress;
        }

        public void SetOnCompleteCallback(System.Action callback)
        {
            _onComplete = callback;
        }

        public void ClearOnCompleteCallback()
        {
            _onComplete = null;
        }

        public void Restart()
        {
            if (!IsValid) return;

            _audioSource.time = 0f;
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            _playStartTime = Time.time;
        }

        public bool IsFinished()
        {
            return !IsValid || (!_audioSource.isPlaying && !_isPaused);
        }

        internal void Update()
        {
            if (!IsValid) return;

            if (IsFinished() && _onComplete != null)
            {
                var callback = _onComplete;
                _onComplete = null;
                callback.Invoke();
            }

            if (IsFinished() && !_audioSource.loop)
            {
                Stop();
            }
        }

        private void Invalidate()
        {
            _isValid = false;
            _audioSource = null;
            _manager = null;
            _onComplete = null;
            
            if (_fadeCoroutine != null)
            {
                _manager?.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private IEnumerator FadeCoroutine(float targetVolume, float duration)
        {
            if (!IsValid) yield break;

            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration && IsValid)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                ApplyVolumeSettings();
                yield return null;
            }

            if (IsValid)
            {
                _audioSource.volume = targetVolume;
                ApplyVolumeSettings();
                
                if (targetVolume <= 0f)
                {
                    Stop();
                }
            }

            _fadeCoroutine = null;
        }

        public void ApplyVolumeSettings()
        {
            if (!IsValid) return;

            float categoryVolume = _manager.GetCategoryVolume(_category);
            float masterVolume = _manager.MasterVolume;
            bool isMuted = _manager.IsMuted;

            float finalVolume = _originalVolume * categoryVolume * masterVolume * (isMuted ? 0f : 1f);
            _audioSource.volume = finalVolume;
        }
    }
}