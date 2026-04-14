// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using ThaumielMapEditor.API.Attributes;

namespace ThaumielMapEditor
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Config
    {
        public bool Debug { get; set; }
        public bool EnableCreditTags { get; set; } = true;
        public bool EnableServerTracking { get; set; } = true;

        [Description("The delay for the DoorLink coroutine (In seconds) if 0 it will run every frame.")]
        public float DoorPollingDelay { get; set; } = 0.1f;

        public List<string> SchematiclodBlacklist { get; set; } = [];

        public string GithubToken { get; set; } = string.Empty;
        public string AudioPath { get; set; } = string.Empty;

        [Description("Use || to randomly load a map use && to load all inline maps. Example: 'Load::MAPNAME||MAPNAME1' will load either MAPNAME or MAPNAME1 but not both. 'Load::MAPNAME&&MAPNAME1' will load both MAPNAME and MAPNAME1")]
        public List<string> WaitingForPlayers { get; set; } = [];
        public List<string> RoundStarted { get; set; } = [];
        public List<string> DecontaminationStarted { get; set; } = [];
        public List<string> WarheadStarted { get; set; } = [];
        public List<string> WarheadDetonated { get; set; } = [];

        [Description("These schematics when loaded will play the specified animation by its name. Key: Schematic name. Value: Animation name")]
        public Dictionary<string, string> SchematicAnimationPlayOnLoad { get; set; } = new()
        {
            ["ExampleSchematicName"] = "ExampleAnimationName"
        };
    }
}