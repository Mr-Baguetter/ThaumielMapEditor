// -----------------------------------------------------------------------
// <copyright file="ColliderClasses.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ThaumielMapEditor.API.Components.Tools.Helpers
{
    [Serializable]
    public class ColliderClasses
    {
        public List<PlayAudio> PlayAudio { get; set; } = [];
        public List<RunCommand> RunCommand { get; set; } = [];
        public List<GiveEffect> GiveEffect { get; set; } = [];
        public List<RemoveEffect> RemoveEffect { get; set; } = [];
        public List<PlayAnimation> PlayAnimation { get; set; } = [];
    }
}