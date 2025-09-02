using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AudioEffectKit
{
    public abstract class AudioEffectBase : IAudioEffect
    {
        protected List<AudioCategory> _supportedCategories = new List<AudioCategory>();
        protected bool _isEnabled = true;
        protected int _priority = 100;
        protected string _effectName = "BaseEffect";
        protected Dictionary<AudioSource, object> _effectData = new Dictionary<AudioSource, object>();

        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }
        
        public List<AudioCategory> SupportedCategories => _supportedCategories.ToList();
        
        public int Priority 
        { 
            get => _priority; 
            set => _priority = value; 
        }
        
        public string EffectName => _effectName;

        public virtual void OnAudioPlay(AudioSource source, AudioClipData clipData)
        {
            if (!_isEnabled || !CanProcess(clipData.Category)) return;
            
            var data = CreateEffectData(source);
            if (data != null)
            {
                _effectData[source] = data;
            }
            
            ProcessAudio(source);
        }

        public virtual void OnAudioStop(AudioSource source)
        {
            if (!_isEnabled) return;
            
            CleanupEffectData(source);
            _effectData.Remove(source);
        }

        public virtual void OnAudioUpdate(AudioSource source, float deltaTime)
        {
            if (!_isEnabled || !_effectData.ContainsKey(source)) return;
            
            ProcessAudio(source);
        }

        public virtual bool CanProcess(AudioCategory category)
        {
            return CheckCategorySupport(category);
        }

        public virtual int GetPriority()
        {
            return _priority;
        }

        public virtual string GetEffectName()
        {
            return _effectName;
        }

        public void AddSupportedCategory(AudioCategory category)
        {
            if (!_supportedCategories.Contains(category))
            {
                _supportedCategories.Add(category);
            }
        }

        public void RemoveSupportedCategory(AudioCategory category)
        {
            _supportedCategories.Remove(category);
        }

        protected virtual void Initialize()
        {
            // Override in derived classes
        }

        protected virtual void ProcessAudio(AudioSource source)
        {
            // Override in derived classes for specific audio processing
        }

        protected virtual object CreateEffectData(AudioSource source)
        {
            // Override in derived classes to create effect-specific data
            return null;
        }

        protected virtual void CleanupEffectData(AudioSource source)
        {
            // Override in derived classes to cleanup effect-specific data
        }

        protected T GetEffectData<T>(AudioSource source) where T : class
        {
            if (_effectData.TryGetValue(source, out var data))
            {
                return data as T;
            }
            return null;
        }

        protected void SetEffectData(AudioSource source, object data)
        {
            _effectData[source] = data;
        }

        private bool CheckCategorySupport(AudioCategory category)
        {
            return _supportedCategories.Count == 0 || _supportedCategories.Contains(category);
        }

        private void CleanupInvalidData()
        {
            var invalidSources = _effectData.Keys.Where(source => source == null || !source.gameObject.activeInHierarchy).ToList();
            foreach (var source in invalidSources)
            {
                _effectData.Remove(source);
            }
        }
    }
}