using System;

namespace ThaumielMapEditor.API.Enums
{
    [Flags]
    public enum TeleporterFlags
    {
        None = 0,
        AllowPickups = 1,
        AllowPlayers = 2,
        AllowProjectiles = 4
    }
}