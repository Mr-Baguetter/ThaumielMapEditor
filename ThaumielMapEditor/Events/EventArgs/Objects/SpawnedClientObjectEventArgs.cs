using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks.ClientSide;

namespace ThaumielMapEditor.Events.EventArgs.Objects
{
    public class SpawnedClientObjectEventArgs : System.EventArgs
    {
        public ClientSideObjectBase Object { get; }
        public Player Player { get; }

        public SpawnedClientObjectEventArgs(ClientSideObjectBase @object, Player player)
        {
            Object = @object;
            Player = player;
        }
    }
}