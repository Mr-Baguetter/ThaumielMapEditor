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
    public class LogManager
    {
        public class Log
        {
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime LogTime { get; set; }
        }

        [Serializable]
        private class ContentPayload
        {
            public string content;
            public ContentPayload(string c) => content = c;
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

        public static void Error(string message, bool sendlog = true)
        {
            string formattedMessage = FormatLogMessage(message);
            Logger.Error(formattedMessage);
            Log log = new() { LogLevel = LogLevel.Error, Message = formattedMessage, LogTime = DateTime.Now };
            Logs.Add(log);
            LogCreated?.Invoke(log);

            if (sendlog)
                Timing.RunCoroutine(UploadToEndPointCoroutine(log));
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

        public static IEnumerator<float> SendLogsCoroutine(LogLevel level)
        {
            if (string.IsNullOrWhiteSpace(Main.Instance.Config.LogsWebhook))
            {
                Error("Logs webhook URL is empty.");
                yield break;
            }

            List<Log> filteredLogs = Logs.Where(l => l.LogLevel == level).ToList();

            if (filteredLogs.Count == 0)
                yield break;

            IEnumerable<string> formattedLogs = filteredLogs.Select(log => $"[{log.LogTime:yyyy-MM-dd HH:mm:ss}] {log.Message}");
            string logsContent = string.Join("\n", formattedLogs);

            List<string> messages = [];

            string header = $"**{level} Logs:**\n```\n";
            string footer = "\n```";
            int chunkSize = 1990;
            int availableSpace = chunkSize - header.Length - footer.Length;
            if (availableSpace <= 0)
                availableSpace = 1500;

            if (logsContent.Length <= availableSpace)
            {
                messages.Add($"{header}{logsContent}{footer}");
            }
            else
            {
                int pos = 0;
                while (pos < logsContent.Length)
                {
                    int len = Math.Min(availableSpace, logsContent.Length - pos);
                    if (pos + len < logsContent.Length)
                    {
                        int lastNl = logsContent.LastIndexOf('\n', pos + len - 1, len);
                        if (lastNl > pos)
                            len = lastNl - pos + 1;
                    }

                    string chunk = logsContent.Substring(pos, len).TrimEnd('\n', '\r');

                    messages.Add(pos == 0 ? $"{header}{chunk}{footer}" : $"```\n{chunk}{footer}");

                    pos += len;
                }
            }

            for (int i = 0; i < messages.Count; i++)
            {
                ContentPayload payload = new(messages[i]);
                string jsonPayload = JsonUtility.ToJson(payload);

                using (UnityWebRequest request = new(Main.Instance.Config.LogsWebhook, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");

                    yield return Timing.WaitUntilDone(request.SendWebRequest());

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        long code = request.responseCode;
                        string resp = request.downloadHandler?.text ?? "<no body>";
                        Error($"Failed to send Discord webhook: HTTP {code} - {request.error}. Response: {resp}");
                    }
                    else
                        Info($"Successfully sent logs to Discord (part {i + 1}/{messages.Count})");
                }

                if (i < messages.Count - 1)
                    yield return Timing.WaitForSeconds(1f);
            }
        }

        public static IEnumerator<float> UploadToEndPointCoroutine(Log log)
        {
            using UnityWebRequest request = new("https://thaumiel.thaumiel-servers.workers.dev/logs/upload", "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Token", Main.Instance.Config.APIToken);
            request.SetRequestHeader("Message", log.Message);
            request.SetRequestHeader("Level", log.LogLevel.ToString());
            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
            {
                Warn($"Failed to upload log to enpoint. {request.error}");
                yield break;
            }
        }
    }
}