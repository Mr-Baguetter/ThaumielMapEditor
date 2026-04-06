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
        public List<string> SchematiclodBlacklist { get; set; } = [];
        public string GithubToken { get; set; } = string.Empty;

        public List<string> LoadOnWaitingForPlayers { get; set; } = [];
        public List<string> LoadOnRoundStart { get; set; } = [];
        public List<string> LoadOnDecom { get; set; } = [];

        [Description("These schematics when loaded will play the specified animation by its name. Key: Schematic name. Value: Animation name")]
        public Dictionary<string, string> SchematicAnimationPlayOnLoad { get; set; } = new()
        {
            ["ExampleSchematicName"] = "ExampleAnimationName"
        };
    }
}