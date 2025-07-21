using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Utilities
{
    // Add whatever specific log system you need here
    public enum LogSystem {
        CSVImporter,
    }

    public static class CustomLogger {
        // Toggle to enable/disable logging globally
        public static bool IsLoggingEnabled = true;
        
        private static readonly string baseLogDir = Path.Combine(Application.persistentDataPath, "Logs");
        private static Dictionary<(LogSystem, string), string> logPaths = new();

        /// <summary>
        /// Initialize logging for a system. Clears existing log file if clear=true.
        /// </summary>
        public static void Init(LogSystem system, string fileName = "system_log.txt", bool clear = true)
        {
            if (!IsLoggingEnabled) return;

            string path = GetLogPath(system, fileName);
            if (clear && File.Exists(path))
                File.Delete(path);

            Log(system, "=== Log Started ===", fileName);
        }

        /// <summary>
        /// Log an info message.
        /// </summary>
        public static void Log(LogSystem system, string message, string fileName = "system_log.txt")
        {
            if (!IsLoggingEnabled) return;

            string path = GetLogPath(system, fileName);
            File.AppendAllText(path, $"{System.DateTime.Now:HH:mm:ss} [INFO] {message}\n");
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        public static void LogError(LogSystem system, string message, string fileName = "system_log.txt")
        {
            if (!IsLoggingEnabled) return;

            string path = GetLogPath(system, fileName);
            File.AppendAllText(path, $"{System.DateTime.Now:HH:mm:ss} [ERROR] {message}\n");
        }

        /// <summary>
        /// Get or create the full log file path for a given system and filename.
        /// </summary>
        private static string GetLogPath(LogSystem system, string fileName)
        {
            var key = (system, fileName);
            if (!logPaths.ContainsKey(key))
            {
                string systemFolder = Path.Combine(baseLogDir, system.ToString());

                if (!Directory.Exists(systemFolder))
                    Directory.CreateDirectory(systemFolder);

                string fullPath = Path.Combine(systemFolder, fileName);
                logPaths[key] = fullPath;
            }
            return logPaths[key];
        }
    }
}