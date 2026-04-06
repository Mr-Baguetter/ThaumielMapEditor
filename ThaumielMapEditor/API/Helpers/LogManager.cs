// -----------------------------------------------------------------------
// <copyright file="LogManager.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Discord;

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
            Log log = new()
            {
                LogLevel = LogLevel.Info,
                Message = formattedMessage,
                LogTime = DateTime.Now
            };

            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Debug(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Debug(formattedMessage, Main.Instance.Config.Debug);
            Log log = new()
            {
                LogLevel = LogLevel.Debug,
                Message = formattedMessage,
                LogTime = DateTime.Now
            };

            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Warn(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Warn(formattedMessage);
            Log log = new()
            {
                LogLevel = LogLevel.Warn,
                Message = formattedMessage,
                LogTime = DateTime.Now
            };

            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Error(string message)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Error(formattedMessage);
            Log log = new()
            {
                LogLevel = LogLevel.Error,
                Message = formattedMessage,
                LogTime = DateTime.Now
            };

            Logs.Add(log);
            LogCreated?.Invoke(log);
        }

        public static void Updater(string message)
        {
            Logger.Raw($"[Updater] [{Main.Instance.GetType().Assembly.GetName().Name}] {message}", ConsoleColor.Blue);
            Log log = new()
            {
                LogLevel = LogLevel.Info,
                Message = message,
                LogTime = DateTime.Now
            };
            
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