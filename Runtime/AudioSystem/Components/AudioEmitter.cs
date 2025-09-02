using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AudioEffectKit
{
    public class AudioEmitter : MonoBehaviour
    {
        [SerializeField] private List<string> _soundNames = new List<string>();
        [SerializeField] private bool _playOnStart = false;
        [SerializeField] private bool _playOnEnable = false;
        [SerializeField] private float _delay = 0f;
        [SerializeField] private AudioCategory _overrideCategory = AudioCategory.SFX;
        [SerializeField] private bool _randomizePitch = false;
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.9f, 1.1f);
        [SerializeField] private bool _useTransformPosition = true;
        
        private AudioHandle _currentHandle;
        private int _lastPlayedIndex = -1;

        public bool IsPlaying => _currentHandle != null && _currentHandle.IsPlaying;
        public AudioHandle CurrentHandle => _currentHandle;
        public int SoundCount => _soundNames.Count;
        public List<string> SoundNames => new List<string>(_soundNames);

        private void Start()
        {
            if (_playOnStart)
            {
                if (_delay > 0f)
                {
                    StartCoroutine(PlayDelayed(_soundNames.Count > 0 ? _soundNames[0] : "", _delay));
                }
                else
                {
                    PlaySound();
                }
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable && !_playOnStart)
            {
                if (_delay > 0f)
                {
                    StartCoroutine(PlayDelayed(_soundNames.Count > 0 ? _soundNames[0] : "", _delay));
                }
                else
                {
                    PlaySound();
                }
            }
        }

        private void OnDisable()
        {
            StopSound();
        }

        public AudioHandle PlaySound(int index = 0)
        {
            if (!IsValidIndex(index)) return null;

            string soundName = _soundNames[index];
            return PlaySound(soundName);
        }

        public AudioHandle PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName) || !AudioManager.Instance.HasSound(soundName))
            {
                Debug.LogWarning($"AudioEmitter: Sound '{soundName}' not found in audio database.");
                return null;
            }

            StopSound();

            Vector3 position = GetPlayPosition();
            _currentHandle = AudioManager.Instance.PlaySound(soundName, position);

            if (_currentHandle != null && _randomizePitch)
            {
                _currentHandle.Pitch = GetRandomizedPitch();
            }

            return _currentHandle;
        }

        public AudioHandle PlayRandomSound()
        {
            if (_soundNames.Count == 0) return null;

            int randomIndex = Random.Range(0, _soundNames.Count);
            return PlaySound(randomIndex);
        }

        public AudioHandle PlayNextSound()
        {
            if (_soundNames.Count == 0) return null;

            _lastPlayedIndex = (_lastPlayedIndex + 1) % _soundNames.Count;
            return PlaySound(_lastPlayedIndex);
        }

        public void StopSound()
        {
            if (_currentHandle != null && _currentHandle.IsValid)
            {
                _currentHandle.Stop();
                _currentHandle = null;
            }
        }

        public void AddSound(string soundName)
        {
            if (!string.IsNullOrEmpty(soundName) && !_soundNames.Contains(soundName))
            {
                _soundNames.Add(soundName);
            }
        }

        public void RemoveSound(string soundName)
        {
            _soundNames.Remove(soundName);
        }

        public void ClearSounds()
        {
            _soundNames.Clear();
        }

        public void SetSounds(List<string> soundNames)
        {
            _soundNames = new List<string>(soundNames ?? new List<string>());
        }

        private IEnumerator PlayDelayed(string soundName, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (!string.IsNullOrEmpty(soundName))
            {
                PlaySound(soundName);
            }
            else if (_soundNames.Count > 0)
            {
                PlaySound(0);
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _soundNames.Count;
        }

        private float GetRandomizedPitch()
        {
            return Random.Range(_pitchRange.x, _pitchRange.y);
        }

        private Vector3 GetPlayPosition()
        {
            return _useTransformPosition ? transform.position : Vector3.zero;
        }
    }
}