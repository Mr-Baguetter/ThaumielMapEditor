using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandSystem;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;
using ThaumielMapEditor.API.Serialization;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace ThaumielMapEditor.Commands.Admin
{
    public class Save : ISubCommand
    {
        public string Name => "save";

        public string VisibleArgs => "<Map Name>";

        public int RequiredArgsCount => 1;

        public string Description => "Saves the current spawned schematics into a map file";

        public string[] Aliases => [""];

        public string RequiredPermission => "tme.save";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            MapData map = new();
            if (!Player.TryGet(sender, out var player))
            {
                response = "You must be a player to run this command!";
                return false;
            }
            
            if (player.Room == null)
            {
                response = $"You must be in a room to run this!";
                return false;
            }

            map.Room = player.Room;
            map.FileName = arguments[0];
            foreach (SchematicData schematic in SchematicLoader.SpawnedSchematics)
            {
                Vector3 pos = player.Room.LocalPosition(schematic.Position);
                map.Schematics.Add((pos, schematic.FileName));
            }

            SchematicLoader.SaveMap(map);
            response = $"Saved map {arguments[0]}.";
            return true;
        }
    }
}