using System.Linq;
using LabApi.Events.Handlers;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;

namespace ThaumielMapEditor.Events
{
    public class ServerHandler
    {
        public static void Register()
        {
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.LczDecontaminationStarted += OnDecom;
        }

        public static void Unregister()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.LczDecontaminationStarted -= OnDecom; 
        }

        private static void OnWaitingForPlayers()
        {
            PrefabHelper.RegisterPrefabs();

            foreach (string name in Main.Instance.Config.LoadOnWaitingForPlayers)
            {
                SerializableMap? map = SchematicLoader.Maps.FirstOrDefault(s => s.FileName.ToLower() == name.ToLower());
                if (map is null)
                {
                    LogManager.Warn($"Map name {name} is invalid!");
                    continue;
                }

                LogManager.Debug($"Loaded map {name} when Waiting for players");
                SchematicLoader.SpawnMap(map);
            }
        }

        private static void OnRoundStart()
        {
            foreach (string name in Main.Instance.Config.LoadOnRoundStart)
            {
                SerializableMap? map = SchematicLoader.Maps.FirstOrDefault(s => s.FileName.ToLower() == name.ToLower());
                if (map is null)
                {
                    LogManager.Warn($"Map name {name} is invalid!");
                    continue;
                }

                LogManager.Debug($"Loaded map {name} on Round Start");
                SchematicLoader.SpawnMap(map);
            }
        }

        private static void OnDecom()
        {
            foreach (string name in Main.Instance.Config.LoadOnDecom)
            {
                SerializableMap? map = SchematicLoader.Maps.FirstOrDefault(s => s.FileName.ToLower() == name.ToLower());
                if (map is null)
                {
                    LogManager.Warn($"Map name {name} is invalid!");
                    continue;
                }

                LogManager.Debug($"Loaded map {name} during Decontamination");
                SchematicLoader.SpawnMap(map);
            }
        }
    }
}