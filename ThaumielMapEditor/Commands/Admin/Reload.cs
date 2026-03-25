using CommandSystem;
using System.Collections.Generic;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
    public class Reload : ISubCommand
    {
        public string Name => "reload";

        public string VisibleArgs => "";

        public int RequiredArgsCount => 0;

        public string Description => "Reloads all schematics";

        public string[] Aliases => ["re"];

        public string RequiredPermission => "tme.reload";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            SchematicLoader.ReloadSchematics();
            response = "Reloaded.";
            return true;
        }
    }
}
