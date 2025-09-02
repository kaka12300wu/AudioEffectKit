using UnityEngine;

namespace AudioEffectKit
{
    public interface IAudioEffect
    {
        void OnAudioPlay(AudioSource source, AudioClipData clipData);
        void OnAudioStop(AudioSource source);
        void OnAudioUpdate(AudioSource source, float deltaTime);
        bool CanProcess(AudioCategory category);
        int GetPriority();
        string GetEffectName();
    }
}