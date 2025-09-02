using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioEffectKit
{
    public static class AudioDebugger
    {
        private static bool _enableDebugLog = false;
        private static Dictionary<AudioCategory, int> _playCount = new Dictionary<AudioCategory, int>();
        private static Dictionary<string, float> _lastPlayTime = new Dictionary<string, float>();
        private static List<string> _debugMessages = new List<string>();
        private static int _maxDebugMessages = 100;

        public static bool EnableDebugLog
        {
            get => _enableDebugLog;
            set => _enableDebugLog = value;
        }

        public static Dictionary<AudioCategory, int> PlayCount => new Dictionary<AudioCategory, int>(_playCount);
        public static List<string> DebugMessages => new List<string>(_debugMessages);

        public static void LogSoundPlay(string soundName, AudioCategory category)
        {
            if (!_enableDebugLog) return;

            if (!_playCount.ContainsKey(category))
            {
                _playCount[category] = 0;
            }
            _playCount[category]++;

            _lastPlayTime[soundName] = Time.time;

            string message = $"[{FormatTimestamp()}] PLAY: {soundName} ({category})";
            AddDebugMessage(message);
            Debug.Log($"AudioDebugger: {message}");
        }

        public static void LogSoundStop(string soundName, AudioCategory category)
        {
            if (!_enableDebugLog) return;

            string message = $"[{FormatTimestamp()}] STOP: {soundName} ({category})";
            AddDebugMessage(message);
            Debug.Log($"AudioDebugger: {message}");
        }

        public static void LogError(string message)
        {
            string formattedMessage = $"[{FormatTimestamp()}] ERROR: {message}";
            AddDebugMessage(formattedMessage);
            Debug.LogError($"AudioDebugger: {formattedMessage}");
        }

        public static void LogWarning(string message)
        {
            string formattedMessage = $"[{FormatTimestamp()}] WARNING: {message}";
            AddDebugMessage(formattedMessage);
            Debug.LogWarning($"AudioDebugger: {formattedMessage}");
        }

        public static int GetCategoryPlayCount(AudioCategory category)
        {
            return _playCount.TryGetValue(category, out var count) ? count : 0;
        }

        public static void ResetStatistics()
        {
            _playCount.Clear();
            _lastPlayTime.Clear();
            _debugMessages.Clear();
        }

        public static string GetDebugReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Audio Debug Report ===");
            report.AppendLine($"Debug Log Enabled: {_enableDebugLog}");
            report.AppendLine($"Total Categories: {_playCount.Count}");
            
            foreach (var kvp in _playCount.OrderByDescending(x => x.Value))
            {
                report.AppendLine($"  {kvp.Key}: {kvp.Value} plays");
            }

            report.AppendLine($"Recent Messages ({_debugMessages.Count}):");
            foreach (var message in _debugMessages.TakeLast(10))
            {
                report.AppendLine($"  {message}");
            }

            return report.ToString();
        }

        public static void ClearDebugMessages()
        {
            _debugMessages.Clear();
        }

        private static void AddDebugMessage(string message)
        {
            _debugMessages.Add(message);
            CheckMessageCapacity();
        }

        private static string FormatTimestamp()
        {
            return Time.time.ToString("F2");
        }

        private static void CheckMessageCapacity()
        {
            while (_debugMessages.Count > _maxDebugMessages)
            {
                _debugMessages.RemoveAt(0);
            }
        }
    }
}