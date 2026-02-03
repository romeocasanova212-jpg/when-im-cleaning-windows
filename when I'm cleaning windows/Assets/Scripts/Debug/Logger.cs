using UnityEngine;

namespace WhenImCleaningWindows.Debugging
{
    /// <summary>
    /// Centralized logging utility for the game.
    /// Provides consistent logging with optional debug console integration.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log an informational message.
        /// </summary>
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
            DebugConsole.DebugLog(message);
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
            DebugConsole.DebugLog($"WARNING: {message}");
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
            DebugConsole.DebugLog($"ERROR: {message}");
        }
    }
}







