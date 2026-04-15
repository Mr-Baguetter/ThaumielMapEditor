using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;

namespace ThaumielMapEditor.Events.EventArgs.Objects
{
    public class DestroyedServerObjectEventArgs : System.EventArgs
    {
        public ServerObject Object { get; }

        public DestroyedServerObjectEventArgs(ServerObject @object)
        {
            Object = @object;
        }
    }
}