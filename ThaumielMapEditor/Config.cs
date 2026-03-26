using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThaumielMapEditor.API.Attributes;

namespace ThaumielMapEditor
{
    [DoNotParse]
    public class Config
    {
        public bool Debug { get; set; }
        public string LogsWebhook { get; set; } = string.Empty;
        public string APIToken { get; set; } = string.Empty;

        public List<string> LoadOnWaitingForPlayers { get; set; } = [];
        public List<string> LoadOnRoundStart { get; set; } = [];
        public List<string> LoadOnDecom { get; set; } = [];
    }
}