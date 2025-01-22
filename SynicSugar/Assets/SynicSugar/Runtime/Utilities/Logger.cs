using System;
using System.Diagnostics;

namespace SynicSugar
{
    internal class Logger
    {
        [Conditional("SYNICSUGAR_LOG")] 
        internal static void Log(string methodName, string message)
        {
            UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [INFO] {methodName}: {message}");
        }
        [Conditional("SYNICSUGAR_LOG")] 
        internal static void Log(string methodName, Result result)
        {
            UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [INFO] {methodName} Result: {result}");
        }
        [Conditional("SYNICSUGAR_LOG")] 
        internal static void Log(string methodName, string message, Result result)
        {
            UnityEngine.Debug.Log($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [INFO] {methodName}: {message} Result: {result}");
        }

        internal static void LogWarning(string methodName, string message)
        {
            UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [WARN] {methodName}: {message}");
        }
        internal static void LogWarning(string methodName, Result result)
        {
            UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [WARN] {methodName} Result: {result}");
        }
        internal static void LogWarning(string methodName, string message, Result result)
        {
            UnityEngine.Debug.LogWarning($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [WARN] {methodName}: {message} Result: {result}");
        }
        
        internal static void LogError(string methodName, string message)
        {
            UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [ERROR] {methodName}: {message}");
        }
        internal static void LogError(string methodName, Result result)
        {
            UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [ERROR] {methodName} Result: {result}");
        }
        internal static void LogError(string methodName, string message, Result result)
        {
            UnityEngine.Debug.LogError($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [ERROR] {methodName}: {message} Result: {result}");
        }
    }
}