// -----------------------------------------------------------------------
// <copyright file="Effect.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace ThaumielMapEditor.API.Components.Tools.Helpers
{
    [Serializable]
    public class GiveEffect
    {
        public int Intensity { get; set; }
        public float Duration { get; set; }
        public string EffectName { get; set; } = string.Empty;
    }

    [Serializable]
    public class RemoveEffect
    {
        public int Intensity { get; set; }
        public string EffectName { get; set; } = string.Empty;
    }
}