// -----------------------------------------------------------------------
// <copyright file="LoggingBlock.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using Discord;
using LabApi.Features.Wrappers;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class LoggingBlock : BlockBase
    {
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Text { get; set; } = string.Empty;

        public override void Execute()
        {
            Log(Text);
        }

        public override void Execute(Player player)
        {
            Log(Text);
        }

        public override void Execute(object obj)
        {
            Log(Text);
        }

        private void Log(string message)
        {
            switch (Level)
            {
                case LogLevel.Debug:
                    LogManager.Debug(message);
                    break;
                case LogLevel.Info:
                    LogManager.Info(message);
                    break;
                case LogLevel.Warn:
                    LogManager.Warn(message);
                    break;
                case LogLevel.Error:
                    LogManager.Error(message);
                    break;
            }
        }
    }
}