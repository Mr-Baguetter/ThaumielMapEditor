using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Discord;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace ThaumielMapEditor.API.Helpers
{
    internal class LogManager
    {
        public class Log
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime LogTime { get; set; }
        }

        public static List<Log> Logs = [];
        public static event Action<Log>? LogCreated;

        public static void Info(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Info(formattedMessage);
            Log log = new() { LogLevel = LogLevel.Info, Message = formattedMessage, LogTime = DateTime.Now };
            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Debug(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Debug(formattedMessage, Main.Instance.Config.Debug);
            Log log = new() { LogLevel = LogLevel.Debug, Message = formattedMessage, LogTime = DateTime.Now };
            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Warn(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Warn(formattedMessage);
            Log log = new() { LogLevel = LogLevel.Warn, Message = formattedMessage, LogTime = DateTime.Now };
            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Error(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Error(formattedMessage);
            Log log = new() { LogLevel = LogLevel.Error, Message = formattedMessage, LogTime = DateTime.Now };
            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        private static string FormatLogMessage(string message)
        {
            StackTrace stackTrace = new(true);
            StackFrame? frame = stackTrace.GetFrame(2);
            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                if (method?.DeclaringType != null)
                {
                    string className;
                    if (method.IsStatic)
                    {
                        className = method.DeclaringType.FullName + $".{method.Name}()" ?? method.DeclaringType.Name + $".{method.Name}()";
                    }
                    else
                        className = method.DeclaringType.FullName + $"::{method.Name}()" ?? method.DeclaringType.Name + $"::{method.Name}()";

                    return $"[{className}] {message}";
                }
            }

            return $"[Unknown] {message}";
        }
    }
}