using System.Collections.Generic;
using ThaumielMapEditor.API.Attributes;

namespace ThaumielMapEditor
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Config
    {
        public bool Debug { get; set; }
        public string LogsWebhook { get; set; } = string.Empty;
        public string APIToken { get; set; } = string.Empty;
        public List<string> SchematiclodBlacklist { get; set; } = [];

        public List<string> LoadOnWaitingForPlayers { get; set; } = [];
        public List<string> LoadOnRoundStart { get; set; } = [];
        public List<string> LoadOnDecom { get; set; } = [];
    }
}