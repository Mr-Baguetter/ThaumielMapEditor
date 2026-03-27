using LabApi.Features.Wrappers;
using MapGeneration;

namespace ThaumielMapEditor.API.Extensions
{
    public static class RoomExtensions
    {
        /// <summary>
        /// Gets the closest room to the <see cref="Vector3"/> <paramref name="pos"/>
        /// </summary>
        /// <param name="pos">The position to get the closest room from</param>
        /// <returns><see cref="RoomIdentifier"/> if a room was found else returns <see langword="null"/> if no room was found</returns>
        public static RoomIdentifier GetClosestRoomToPosition(Vector3 pos)
        {
            if (Room.List == null || Room.List.Count == 0)
                return null;

            RoomIdentifier closest = null;
            float closestSqrDist = float.MaxValue;

            foreach (Room room in Room.List)
            {
                if (room == null || room.Base == null)
                    continue;

                float sqrDist = (room.Position - pos).sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closest = room.Base;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns the local space position, based on a world space position.
        /// </summary>
        /// <param name="room">The room instance this method extends.</param>
        /// <param name="position">World position.</param>
        /// <returns>Local position, based on the room.</returns>
        public static Vector3 LocalPosition(this Room room, Vector3 position) => room.Transform.InverseTransformPoint(position);

        /// <summary>
        /// Returns the World position, based on a local space position.
        /// </summary>
        /// <param name="room">The room instance this method extends.</param>
        /// <param name="offset">Local position.</param>
        /// <returns>World position, based on the room.</returns>
        public static Vector3 WorldPosition(this Room room, Vector3 offset) => room.Transform.TransformPoint(offset);
    }
}