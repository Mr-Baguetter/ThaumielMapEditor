using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;

namespace ThaumielMapEditor.Events.EventArgs.Objects
{
    public class SpawnedServerObjectEventArgs : System.EventArgs
    {
        public ServerObject Object { get; }

        public SpawnedServerObjectEventArgs(ServerObject @object)
        {
            Object = @object;
        }
    }
}