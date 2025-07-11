using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Unity.Services.LevelPlay
{
    static class LevelPlayLogger
    {
        const string k_Tag = "[LevelPlay]";
        const string k_VerboseLoggingDefine = "ENABLE_UNITY_SERVICES_LEVELPLAY_VERBOSE_LOGGING";
        const string k_UnityAssertions = "UNITY_ASSERTIONS";

        /// <summary>
        /// Logs a message in the console
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(object message)
        {
            Debug.Log($"{k_Tag}: {message}");
        }

        /// <summary>
        /// Logs a warning in the console
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogWarning(object message)
        {
            Debug.LogWarning($"{k_Tag}: {message}");
        }

        /// <summary>
        /// Logs an error in the console
        /// <param name="message">Message to log</param>
        /// </summary>
        public static void LogError(object message)
        {
            Debug.LogError($"{k_Tag}: {message}");
        }

        /// <summary>
        /// Logs an exception in the console
        /// <param name="exception">Exception to log</param>
        /// </summary>
        public static void LogException(Exception exception)
        {
            if (exception == null)
            {
                Debug.LogException(new Exception($"{k_Tag}: null exception"));
                return;
            }

            var formatted = $"{k_Tag}: {exception.GetType()}: {exception.Message}";
            Debug.LogException(new Exception(formatted));
        }

        /// <summary>
        /// Logs an assertion in the console (only available when the definition is enabled)
        /// <param name="message">Message to log</param>
        /// </summary>
        [Conditional(k_UnityAssertions)]
        public static void LogAssertion(object message)
        {
            Debug.LogAssertion($"{k_Tag}: {message}");
        }

        /// <summary>
        /// Logs a verbose log in the console if verbose logging is activated
        /// <param name="message">Message to log</param>
        /// </summary>
#if !ENABLE_UNITY_SERVICES_VERBOSE_LOGGING
        [Conditional(k_VerboseLoggingDefine)]
#endif
        public static void LogVerbose(object message)
        {
            Debug.Log($"{k_Tag}: {message}");
        }
    }
}
