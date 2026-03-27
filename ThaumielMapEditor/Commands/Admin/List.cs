using System.Collections.Generic;
using System.Text;
using CommandSystem;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;
using ThaumielMapEditor.API.Serialization;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class List : ISubCommand
    {
        public string Name => "list";

        public string VisibleArgs => "";

        public int RequiredArgsCount => 0;

        public string Description => "Lists all schematics";

        public string[] Aliases => ["li"];

        public string RequiredPermission => "tme.list";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            foreach (SerializableSchematic schematic in SchematicLoader.LoadedSchematics)
                sb.AppendLine($"- {schematic.FileName}");

            response = sb.ToString();
            return true;
        }
    }
}