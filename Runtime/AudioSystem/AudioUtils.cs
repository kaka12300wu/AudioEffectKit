using UnityEngine;

namespace AudioEffectKit
{
    public static class AudioUtils
    {
        public static float DecibelToLinear(float decibel)
        {
            return Mathf.Pow(10.0f, decibel / 20.0f);
        }

        public static float LinearToDecibel(float linear)
        {
            return 20.0f * Mathf.Log10(Mathf.Max(linear, 0.0001f));
        }

        public static float CalculateDistanceAttenuation(float distance, float minDistance, float maxDistance, AudioRolloffMode rolloffMode)
        {
            if (distance <= minDistance) return 1.0f;
            if (distance >= maxDistance) return 0.0f;

            float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

            switch (rolloffMode)
            {
                case AudioRolloffMode.Linear:
                    return 1.0f - normalizedDistance;
                case AudioRolloffMode.Logarithmic:
                    return 1.0f / (1.0f + normalizedDistance * normalizedDistance);
                default:
                    return 1.0f - normalizedDistance;
            }
        }

        public static float RandomizePitch(float basePitch, Vector2 range)
        {
            return basePitch * Random.Range(range.x, range.y);
        }

        public static bool IsValidSoundName(string name)
        {
            return !string.IsNullOrEmpty(name) && !name.Contains(" ") && name.Length <= 50;
        }

        public static AudioClipData GetCategoryDefaults(AudioCategory category)
        {
            // This would typically be implemented with ScriptableObject defaults
            // For now, return null and let the system use built-in defaults
            return null;
        }

        public static float BlendVolumes(float volume1, float volume2, float blend)
        {
            return Mathf.Lerp(volume1, volume2, Mathf.Clamp01(blend));
        }

        public static float SmoothVolume(float current, float target, float smoothTime, float deltaTime)
        {
            return Mathf.MoveTowards(current, target, deltaTime / smoothTime);
        }
    }
}