// -----------------------------------------------------------------------
// <copyright file="Animation.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace ThaumielMapEditor.API.Components.Tools.Helpers
{
    [Serializable]
    public class PlayAnimation
    {
        public string ResolvedAnimationName { get; set; } = string.Empty;
    }
}