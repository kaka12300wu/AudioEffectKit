using UnityEngine;

namespace AudioEffectKit
{
    [RequireComponent(typeof(AudioListener))]
    public class AudioListener3D : MonoBehaviour
    {
        private AudioListener _listener;
        [SerializeField] private float _dopplerLevel = 1.0f;
        [SerializeField] private AudioVelocityUpdateMode _velocityUpdateMode = AudioVelocityUpdateMode.Auto;
        [SerializeField] private bool _pauseOnFocusLoss = true;
        
        private Vector3 _lastPosition;
        private Vector3 _velocity;

        public float DopplerLevel
        {
            get => _dopplerLevel;
            set
            {
                _dopplerLevel = Mathf.Clamp(value, 0f, 5f);
                ApplyDopplerSettings();
            }
        }

        public AudioVelocityUpdateMode VelocityUpdateMode
        {
            get => _velocityUpdateMode;
            set
            {
                _velocityUpdateMode = value;
                if (_listener != null)
                {
                    _listener.velocityUpdateMode = _velocityUpdateMode;
                }
            }
        }

        public bool PauseOnFocusLoss
        {
            get => _pauseOnFocusLoss;
            set => _pauseOnFocusLoss = value;
        }

        public Vector3 Velocity => _velocity;
        public AudioListener Listener => _listener;

        private void Awake()
        {
            _listener = GetComponent<AudioListener>();
            Initialize();
        }

        private void Update()
        {
            CalculateVelocity();
            ApplyDopplerSettings();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_pauseOnFocusLoss)
            {
                if (hasFocus)
                {
                    AudioManager.Instance.MasterVolume = AudioManager.Instance.MasterVolume;
                }
                else
                {
                    // Temporarily reduce volume when focus is lost
                    AudioManager.Instance.MasterVolume = AudioManager.Instance.MasterVolume * 0.1f;
                }
            }
        }

        public void Initialize()
        {
            if (_listener == null) return;

            _lastPosition = transform.position;
            _listener.velocityUpdateMode = _velocityUpdateMode;
            ApplyDopplerSettings();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public float GetDistanceTo(Vector3 position)
        {
            return Vector3.Distance(transform.position, position);
        }

        private void CalculateVelocity()
        {
            Vector3 currentPosition = transform.position;
            _velocity = (currentPosition - _lastPosition) / Time.deltaTime;
            _lastPosition = currentPosition;
        }

        private void ApplyDopplerSettings()
        {
            if (_listener != null)
            {
                AudioListener.volume = AudioManager.Instance.MasterVolume;
                // Note: Doppler level is now set per AudioSource, not globally
            }
        }
    }
}