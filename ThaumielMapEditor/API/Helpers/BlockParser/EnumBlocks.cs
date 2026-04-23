// -----------------------------------------------------------------------
// <copyright file="EnumBlocks.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class EnumBlock : BlockBase
    {
        public object? Value { get; set; }

        public override object ReturnExecute()
        {
            if (Value is not Enum enum1)
                return null!;

            return enum1.ToString();
        }
    }
}