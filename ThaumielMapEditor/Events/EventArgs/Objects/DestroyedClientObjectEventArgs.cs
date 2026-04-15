using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks.ClientSide;

namespace ThaumielMapEditor.Events.EventArgs.Objects
{
    public class DestroyedClientObjectEventArgs : System.EventArgs
    {
        public ClientSideObjectBase Object { get; }
        public Player Player { get; }

        public DestroyedClientObjectEventArgs(ClientSideObjectBase @object, Player player)
        {
            Object = @object;
            Player = player;
        }
    }
}