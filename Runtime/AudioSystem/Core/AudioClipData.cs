using UnityEngine;
using System;

namespace AudioEffectKit
{
    [Serializable]
    public class AudioClipData
    {
        [SerializeField] private string _name;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private AudioCategory _category;
        [SerializeField] private float _volume = 1.0f;
        [SerializeField] private float _pitch = 1.0f;
        [SerializeField] private bool _loop = false;
        [SerializeField] private bool _is3D = false;
        [SerializeField] private float _spatialBlend = 0.0f;
        [SerializeField] private AnimationCurve _rolloffCurve;
        [SerializeField] private float _maxDistance = 500.0f;
        [SerializeField] private float _minDistance = 1.0f;
        [SerializeField] private int _priority = 128;
        [SerializeField] private bool _bypassEffects = false;
        [SerializeField] private bool _bypassListenerEffects = false;

        public string Name => _name;
        public AudioClip Clip => _clip;
        public AudioCategory Category => _category;
        public float Volume => _volume;
        public float Pitch => _pitch;
        public bool Loop => _loop;
        public bool Is3D => _is3D;
        public float SpatialBlend => _spatialBlend;
        public AnimationCurve RolloffCurve => _rolloffCurve;
        public float MaxDistance => _maxDistance;
        public float MinDistance => _minDistance;
        public int Priority => _priority;
        public bool BypassEffects => _bypassEffects;
        public bool BypassListenerEffects => _bypassListenerEffects;
        public float Duration => _clip != null ? _clip.length : 0f;

        public AudioClipData Clone()
        {
            var clone = new AudioClipData();
            clone._name = _name;
            clone._clip = _clip;
            clone._category = _category;
            clone._volume = _volume;
            clone._pitch = _pitch;
            clone._loop = _loop;
            clone._is3D = _is3D;
            clone._spatialBlend = _spatialBlend;
            clone._rolloffCurve = _rolloffCurve != null ? new AnimationCurve(_rolloffCurve.keys) : null;
            clone._maxDistance = _maxDistance;
            clone._minDistance = _minDistance;
            clone._priority = _priority;
            clone._bypassEffects = _bypassEffects;
            clone._bypassListenerEffects = _bypassListenerEffects;
            return clone;
        }

        public void ApplyToAudioSource(AudioSource source)
        {
            if (source == null || _clip == null) return;

            source.clip = _clip;
            source.volume = _volume;
            source.pitch = _pitch;
            source.loop = _loop;
            source.spatialBlend = _spatialBlend;
            source.maxDistance = _maxDistance;
            source.minDistance = _minDistance;
            source.priority = _priority;
            source.bypassEffects = _bypassEffects;
            source.bypassListenerEffects = _bypassListenerEffects;
            
            if (_rolloffCurve != null && _rolloffCurve.keys.Length > 0)
            {
                source.rolloffMode = AudioRolloffMode.Custom;
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _rolloffCurve);
            }
        }

        public bool IsValid()
        {
            return ValidateAudioClip() && ValidateParameters();
        }

        public void ResetToDefaults()
        {
            _volume = 1.0f;
            _pitch = 1.0f;
            _loop = false;
            _is3D = false;
            _spatialBlend = 0.0f;
            _maxDistance = 500.0f;
            _minDistance = 1.0f;
            _priority = 128;
            _bypassEffects = false;
            _bypassListenerEffects = false;
            _rolloffCurve = null;
        }

        private bool ValidateAudioClip()
        {
            return _clip != null;
        }

        private bool ValidateParameters()
        {
            return _volume >= 0f && _volume <= 1f &&
                   _pitch >= 0.1f && _pitch <= 3f &&
                   _spatialBlend >= 0f && _spatialBlend <= 1f &&
                   _maxDistance > 0f && _minDistance >= 0f &&
                   _priority >= 0 && _priority <= 256;
        }
    }
}