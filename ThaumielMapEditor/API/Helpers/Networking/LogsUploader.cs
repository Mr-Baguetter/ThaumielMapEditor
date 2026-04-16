// -----------------------------------------------------------------------
// <copyright file="LogsUploader.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LabApi.Features;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine.Networking;

namespace ThaumielMapEditor.API.Helpers.Networking
{
    public class LogsUploader
    {
        private class Payload
        {
            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("labapi_version")]
            public string LabApiVersion { get; set; } = string.Empty;

            [JsonPropertyName("plugin_version")]
            public string PluginVersion { get; set; } = string.Empty;

            [JsonPropertyName("log_data")]
            public string LogData { get; set; } = string.Empty;
        }

        public class UploadResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("size_bytes")]
            public int SizeBytes { get; set; }

            [JsonPropertyName("created_at")]
            public string CreatedAt { get; set; } = string.Empty;
        }

        /// <summary>
        /// Runs the <see cref="SendLogsCoroutine"/>.
        /// </summary>
        /// <param name="onComplete">Runs when the <see cref="SendLogsCoroutine"/> has finished.</param>
        /// <returns>The <see cref="CoroutineHandle"/> of the ran <see cref="SendLogsCoroutine"/>.</returns>
        public static CoroutineHandle SendRequest(Action<UploadResponse?> onComplete)
        {
            MECHelper.TryRunCoroutine(SendLogsCoroutine(onComplete), out var handle);
            return handle;
        }

        private static IEnumerator<float> SendLogsCoroutine(Action<UploadResponse?> onComplete)
        {
            Payload payload = new()
            {
                Port = Server.Port,
                LabApiVersion = LabApiProperties.CompiledVersion,
                PluginVersion = Main.Instance.Version.ToString(),
                LogData = string.Join("\n", LogManager.Logs.Select(l => $"[{l.LogTime}] [{l.LogLevel}] {l.Message}"))
            };

            using UnityWebRequest request = new("https://tmelogs.thaumiel-servers.workers.dev/upload", "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
            {
                LogManager.LogShare($"[Error]: Failed to send logs: {request.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            UploadResponse? response = JsonSerializer.Deserialize<UploadResponse>(request.downloadHandler.text);
            if (response == null || !response.Success)
            {
                LogManager.LogShare("[Error]: Failed to upload logs, server returned unsuccessful response");
                onComplete?.Invoke(null);
                yield break;
            }

            onComplete?.Invoke(response);
        }
    }
}