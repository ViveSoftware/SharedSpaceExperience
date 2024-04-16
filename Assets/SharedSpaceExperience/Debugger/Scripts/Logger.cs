using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;

namespace Debugger
{
    public class Logger
    {
        public enum LogLevel
        {
            Debug,
            Warning,
            Error
        }

        public struct LogObject
        {
            public LogLevel level;
            public string className;
            public string methodName;
            public string log;
        }


        // singleton instance
        private static readonly Logger _instance = new();
        public Logger instance
        {
            get
            {
                return _instance;
            }
        }

        // buffer
        private static int consumerCount = 0;
        public const int MAX_BUFFER_SIZE = 128;
        private static ConcurrentQueue<LogObject> buffer = new();

        public static void EnableBuffer()
        {
            consumerCount++;
        }

        public static void DisableBuffer()
        {
            --consumerCount;
            if (consumerCount <= 0)
            {
                // clear buffer
                buffer.Clear();
                consumerCount = 0;
            }
        }

        public static void Log(string log, bool enqueue = true, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            Log(LogLevel.Debug, enqueue, methodName, filePath, log);
        }

        public static void LogWarning(string log, bool enqueue = true, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            Log(LogLevel.Warning, enqueue, methodName, filePath, log);
        }

        public static void LogError(string log, bool enqueue = true, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            Log(LogLevel.Error, enqueue, methodName, filePath, log);
        }

        private static void Log(LogLevel level, bool enqueue, string methodName, string filePath, string log)
        {
            // Get class name from file name. 
            // This assumes the class name is the same as its file name.
#if UNITY_ANDROID
            string className = Path.GetFileNameWithoutExtension(filePath.Replace('\\', '/'));
#else
            string className = Path.GetFileNameWithoutExtension(filePath);
#endif
            // Or get class from stack trace, which may not be efficient
            // string className = GetClassNameFromStackTrace(methodName);

            // format log
            string formattedLog = $"[{className}][{methodName}] {log}";

            // Unity log
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(formattedLog);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedLog);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedLog);
                    break;
            }

            // only push to queue if there are consumers
            if (!enqueue || consumerCount <= 0) return;

            buffer.Enqueue(new LogObject()
            {
                level = level,
                className = className,
                methodName = methodName,
                log = log
            });

            if (buffer.Count > MAX_BUFFER_SIZE)
            {
                // drop old logs
                buffer.TryDequeue(out LogObject droppedLog);
                UnityEngine.Debug.LogError("[Logger] Drop log: " + droppedLog.log);
            }

        }

        private static string GetClassNameFromStackTrace(string methodName)
        {
            if (methodName == "") return "";

            StackTrace stackTrace = new();
            int frameCount = stackTrace.FrameCount;
            for (int i = 0; i < frameCount; ++i)
            {
                MethodBase methodInfo = stackTrace.GetFrame(i).GetMethod();
                if (methodInfo.Name == methodName)
                {
                    return methodInfo.ReflectedType.Name;
                }
            }

            return "";
        }

        public static int GetLogCount()
        {
            return buffer.Count;
        }

        public static bool TryGetLog(out LogObject log)
        {
            return buffer.TryDequeue(out log);
        }

    }
}
