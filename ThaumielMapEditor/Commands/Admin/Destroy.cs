using System.Collections.Generic;
using System.Text;
using CommandSystem;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
    public class Destroy : ISubCommand
    {
        public string Name => "destroy";

        public string VisibleArgs => "<Schematic Id>";

        public int RequiredArgsCount => 1;

        public string Description => "Destroys the specified schematic";

        public string[] Aliases => ["de"];

        public string RequiredPermission => "tme.destroy";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            uint count = 0;
            if (!SchematicLoader.SchematicsById.TryGetValue(uint.Parse(arguments[0]), out var data))
            {
                StringBuilder sb = new();
                sb.AppendLine();
                sb.AppendLine($"No schematic with id {arguments[0]} was found.");
                sb.AppendLine($"Available schematics:");
                foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                    sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");

                response = sb.ToString();
                return false;
            }

            foreach (PrimitiveObject primitive in data.Primitives)
            {
                count = primitive.DespawnForAllPlayers();
            }

            response = $"Destroyed schematic {arguments[0]} for {count} players";
            return true;
        }
    }
}