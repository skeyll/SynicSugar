using System;
using System.Diagnostics;

namespace SynicSugar
{
    internal class Logger
    {
        [Conditional("SYNICSUGAR_LOG")] 
        public static void Log(string methodName, string message)
        {
            UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [INFO] {methodName}: {message}");
        }
        [Conditional("SYNICSUGAR_LOG")] 
        public static void Log(string methodName, string message, Result result)
        {
            UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [INFO] {methodName}: {message} Result: {result}");
        }

        public static void LogWarning(string methodName, string message)
        {
            UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [WARN] {methodName}: {message}");
        }
        public static void LogWarning(string methodName, string message, Result result)
        {
            UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [WARN] {methodName}: {message} Result: {result}");
        }
        
        public static void LogError(string methodName, string message)
        {
            UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [ERROR] {methodName}: {message}");
        }
        public static void LogError(string methodName, string message, Result result)
        {
            UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [ERROR] {methodName}: {message} Result: {result}");
        }
    }
}