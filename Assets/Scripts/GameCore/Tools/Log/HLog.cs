using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tools.Log{
    public enum LogLevel{
        None = 0,
        Info,
        Warning,
        Error
    }

    public static class HLog{
        private const string DefaultTag = "HLog";

        [Conditional("DEBUG_LOG")]
        public static void Log(object message, params object[] args){
            LogLevelFormat(LogLevel.Info, message.ToString(), args: args);
        }

        [Conditional("DEBUG_LOG")]
        public static void LogW(object message, params object[] args){
            LogLevelFormat(LogLevel.Warning, message.ToString(), args: args);
        }

        [Conditional("DEBUG_LOG")]
        public static void LogE(object message, params object[] args){
            LogLevelFormat(LogLevel.Error, message.ToString(), args: args);
        }

        [Conditional("DEBUG_LOG")]
        public static void Log<T>(T owner, object message, params object[] args){
            LogLevelFormat(LogLevel.Info, message.ToString(), owner.GetType().Name, args);
        }

        [Conditional("DEBUG_LOG")]
        public static void LogW<T>(T owner, object message, params object[] args){
            LogLevelFormat(LogLevel.Warning, message.ToString(), owner.GetType().Name, args);
        }

        [Conditional("DEBUG_LOG")]
        public static void LogE<T>(T owner, object message, params object[] args){
            LogLevelFormat(LogLevel.Error, message.ToString(), owner.GetType().Name, args);
        }

        /// <summary>
        ///     拼接日志模板
        /// </summary>
        /// <param name="level"></param>
        /// <param name="originalString"></param>
        /// <param name="owner"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static string SplicingLog(LogLevel level, string originalString, object owner, params object[] args){
            var temp = LogConfig.Instance.template;
            LogConfig.Instance.ColorDic.TryGetValue(level, out var color);
            var tagString = LogConfig.Instance.tag;
            var timeString = LogConfig.Instance.time.Replace("*",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
            var ownerString = owner == null ? "" : LogConfig.Instance.owner.Replace("*", owner.ToString());
            var messageString = LogConfig.Instance.message.Replace("*",
                $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{string.Format(originalString, args)}</color>");

            var replace = temp.Replace("{tag}", tagString)
                .Replace("{time}", timeString)
                .Replace("{owner}", ownerString)
                .Replace("{message}", messageString);

            return replace;
        }

        private static void LogLevelFormat(LogLevel level, string message, object owner = null, params object[] args){
            switch (level){
                case LogLevel.None:
                    Debug.Log(SplicingLog(level, message, owner, args));
                    break;
                case LogLevel.Info:
                    Debug.Log(SplicingLog(level, message, owner, args));
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(SplicingLog(level, message, owner, args));
                    break;
                case LogLevel.Error:
                    Debug.LogError(SplicingLog(level, message, owner, args));
                    break;
            }
        }
    }
}