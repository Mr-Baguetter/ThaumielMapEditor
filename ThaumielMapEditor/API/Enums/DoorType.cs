using ThaumielMapEditor.API.Blocks.ServerObjects;

namespace ThaumielMapEditor.API.Enums
{
    /// <summary>
    /// Defines the types that <see cref="DoorObject"/> can use.
    /// </summary>
    public enum DoorType
    {
        /// <summary>
        /// Light containment zone
        /// </summary>
        Lcz = 1,

        /// <summary>
        /// Heavy containment zone
        /// </summary>
        Hcz = 2,

        /// <summary>
        /// Entrance zone
        /// </summary>
        Ez = 3,

        /// <summary>
        /// The gates used at Gate A, and Gate B
        /// </summary>
        Gate = 4,

        /// <summary>
        /// The containment bulkheads in heavy containment zone
        /// </summary>
        BulkHead = 5
    }
}