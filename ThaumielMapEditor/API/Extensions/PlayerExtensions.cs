using System;
using System.Reflection;
using LabApi.Features.Wrappers;
using LabApiExtensions.Extensions;
using LabApiExtensions.FakeExtension;
using Mirror;
using UnityEngine;

namespace ThaumielMapEditor.API.Extensions
{
    public static class PlayerExtensions
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        public static bool TryGet(this ReferenceHub hub, out Player player) =>
            Player.TryGet(hub.gameObject, out player);
#pragma warning restore CS8601 // Possible null reference assignment.

        public static void SendFakeRPC(this Player player, uint netId, Type type, string functionName, int componentIndex, params object[] objects)
        {
            MethodInfo method = type.GetMethod(functionName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Logger.Error($"Method '{functionName}' not found on type '{type.FullName}'");
                return;
            }

            string longFuncName = FakeRpcExtension.GetLongFuncName(type, method);
            int stableHashCode = longFuncName.GetStableHashCode();

            using NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
            foreach (object obj in objects)
            {
                if (!MirrorWriterExtension.Write(obj.GetType(), obj, networkWriterPooled))
                {
                    Logger.Error($"Not found NetworkWriter for type {obj.GetType()}");
                    return;
                }
            }

            player.Connection.Send(new RpcMessage
            {
                netId = netId,
                componentIndex = (byte)componentIndex,
                functionHash = (ushort)stableHashCode,
                payload = networkWriterPooled.ToArraySegment()
            });
        }
    }
}