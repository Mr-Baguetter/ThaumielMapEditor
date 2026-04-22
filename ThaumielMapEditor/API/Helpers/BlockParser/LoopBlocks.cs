// -----------------------------------------------------------------------
// <copyright file="LoopBlocks.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class RepeatBlock : BlockBase
    {
        public object? Times { get; set; }
        public List<object?> Stack { get; set; } = [];

        public override void Execute(Player player)
        {
            int count = (int)ResolveFloat(Times);
            for (int i = 0; i < count; i++)
            {
                Executor?.Execute(Stack!, player);
            }
        }
    }

    public class WhileUntilBlock : BlockBase
    {
        public string Mode { get; set; } = "WHILE";
        public BlockBase? Condition { get; set; }
        public List<object?> Stack { get; set; } = [];

        public override void Execute(Player player)
        {
            while (ShouldContinue())
            {
                Executor?.Execute(Stack!, player);
            }
        }

        private bool ShouldContinue()
        {
            object? result = Condition?.ReturnExecute();
            bool val = result is bool b && b;
            return Mode == "WHILE" ? val : !val;
        }
    }

    public class ForLoopBlock : BlockBase
    {
        public string VarName { get; set; } = "i";
        public object? From { get; set; }
        public object? To { get; set; }
        public object? By { get; set; }
        public List<object?> Stack { get; set; } = [];

        public override void Execute(Player player)
        {
            float start = ResolveFloat(From);
            float end = ResolveFloat(To);
            float step = ResolveFloat(By);

            for (float i = start; i <= end; i += step)
            {
                if (Executor != null && Executor.Scopes.Count > 0)
                    Executor.Scopes.Peek()[VarName] = i;

                Executor?.Execute(Stack!, player);
            }
        }
    }
}