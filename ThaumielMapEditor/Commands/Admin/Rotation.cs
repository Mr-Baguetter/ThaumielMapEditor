using System.Collections.Generic;
using CommandSystem;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
    public class Rotation : ISubCommand
    {
        public string Name => "rotate";
        public string VisibleArgs => "<Schematic ID>, <X>, <Y>, <Z>";
        public int RequiredArgsCount => 4;
        public string Description => "Grabs the specified schematic";
        public string[] Aliases => ["rot"];
        public string RequiredPermission => "tme.rotate";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {            
            if (!uint.TryParse(arguments[0], out var id))
            {
                response = $"Failed to parse ID. Make sure its a non negative number. You can get all schematics by running this command: 'tme list'";
                return false;
            }

            if (!SchematicLoader.TryGetSchematicById(id, out var schematic))
            {
                response = $"Failed to find schematic with the ID {id}. Run 'tme list' to get all spawned schematics";
                return false;
            }

            if (schematic.Primitive == null)
            {
                response = $"Main schematic primitive is null!";
                return false;
            }

            if (!float.TryParse(arguments[1], out var x))
            {
                response = "Failed to parse X coordinate. Make sure its a number";
                return false;
            }

            if (!float.TryParse(arguments[2], out var y))
            {
                response = "Failed to parse Y coordinate. Make sure its a number";
                return false;
            }

            if (!float.TryParse(arguments[3], out var z))
            {
                response = "Failed to parse Z coordinate. Make sure its a number";
                return false;
            }

            Quaternion rotation = Quaternion.Euler(new(x, y, z));
            Quaternion prevrot = schematic.Primitive.Rotation;
            schematic.Rotation = rotation;
            schematic.Primitive.Rotation = rotation;

            foreach (ServerObject serverObject in schematic.SpawnedServerObjects)
            {
                serverObject.UpdateObject(schematic);
            }

            response = $"Rotated schematic {schematic.FileName} to {rotation} from {prevrot}";
            return true;
        }
    }
}