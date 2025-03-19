using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util {
    public static class Logger {
        public readonly struct LogMessageString {
            public FormattableString Value { get; }

            public LogMessageString(FormattableString value) {
                this.Value = value;
            }

            public static implicit operator LogMessageString(FormattableString value) {
                return new LogMessageString(value);
            }
        
            public static implicit operator LogMessageString(Exception error) {
                return new LogMessageString(FormattableStringFactory.Create(error.ToString()));
            }
        
            public static implicit operator LogMessageString(string message) {
                return new LogMessageString(FormattableStringFactory.Create(message));
            }
        }

        [DebuggerHidden, HideInCallstack]
        public static void Info(
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Log, null, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Info(
            Object context,
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Log, context, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Warn(
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Warning, null, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Warn(
            Object context,
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Warning, context, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Error(
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Error, null, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Error(
            Object context,
            LogMessageString message,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Error, context, message, filePath, memberName);
        }
        
        [DebuggerHidden, HideInCallstack]
        public static void Exception(
            Object context,
            Exception exception,
            [CallerFilePath] string filePath = null, 
            [CallerMemberName] string memberName = null
        ) {
            Log(LogType.Exception, context, exception, filePath, memberName);
        }

        [DebuggerHidden, HideInCallstack]
        private static void Log(LogType logType, Object context, LogMessageString message, string filePath, string memberName) {
            if (!Debug.unityLogger.IsLogTypeAllowed(logType)) {
                return;
            }

            var messageString = message.Value;
            if (messageString == null) {
                return;
            }

            var formattedMessage = messageString.ArgumentCount switch {
                0 => messageString.Format,
                _ => string.Format(messageString.Format, messageString.GetArguments()), 
            };

            var tag = Path.GetFileNameWithoutExtension(filePath);
            if (!string.IsNullOrEmpty(memberName)) {
                tag = tag == null ? memberName : $"{tag}.{memberName}";
            }

            Debug.unityLogger.Log(logType, tag, formattedMessage, context);
        }
    }
}
