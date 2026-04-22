// -----------------------------------------------------------------------
// <copyright file="BlockBase.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Data;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public abstract class BlockBase
    {
        public BlockExecutor? Executor { get; internal set; }

        public virtual void Execute()
        {

        }

        public virtual void Execute(Player player)
        {

        }

        public virtual void Execute(object obj)
        {

        }

        public virtual object ReturnExecute()
        {
            return null!;
        }

        public virtual object ReturnExecute(object obj)
        {
            return null!;
        }

        public virtual object ReturnExecute(SchematicData schematic)
        {
            return null!;
        }
    }
}