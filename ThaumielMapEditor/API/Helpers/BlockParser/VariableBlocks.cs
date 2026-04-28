// -----------------------------------------------------------------------
// <copyright file="VariableBlocks.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class VariableBlock : BlockBase
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class GetVariableBlock : BlockBase
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }

        public override object ReturnExecute()
        {
            return Value!;
        }

        public override object ReturnExecute(object obj)
        {
            return Value!;
        }
    }
}