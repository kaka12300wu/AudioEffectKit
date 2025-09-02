using System;

namespace AudioEffectKit
{
    public static class AudioEvents
    {
        public static event Action<string, AudioCategory> OnSoundPlay;
        public static event Action<string, AudioCategory> OnSoundStop;
        public static event Action<AudioCategory, float> OnCategoryVolumeChanged;
        public static event Action<float> OnMasterVolumeChanged;
        public static event Action<bool> OnMuteStateChanged;

        public static void TriggerSoundPlay(string soundName, AudioCategory category)
        {
            OnSoundPlay?.Invoke(soundName, category);
        }

        public static void TriggerSoundStop(string soundName, AudioCategory category)
        {
            OnSoundStop?.Invoke(soundName, category);
        }

        public static void TriggerCategoryVolumeChanged(AudioCategory category, float volume)
        {
            OnCategoryVolumeChanged?.Invoke(category, volume);
        }

        public static void TriggerMasterVolumeChanged(float volume)
        {
            OnMasterVolumeChanged?.Invoke(volume);
        }

        public static void TriggerMuteStateChanged(bool isMuted)
        {
            OnMuteStateChanged?.Invoke(isMuted);
        }
    }
}