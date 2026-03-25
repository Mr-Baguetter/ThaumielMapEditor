using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandSystem;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;
using ThaumielMapEditor.API.Serialization;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace ThaumielMapEditor.Commands.Admin
{
    public class Spawned : ISubCommand
    {
        public string Name => "spawned";

        public string VisibleArgs => "";

        public int RequiredArgsCount => 0;

        public string Description => "Gets all spawned Schematics";

        public string[] Aliases => ["spd"];

        public string RequiredPermission => "tme.spawned";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            sb.AppendLine($"Available schematics:");
            foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");

            response = sb.ToString();
            return true;
        }
    }
}