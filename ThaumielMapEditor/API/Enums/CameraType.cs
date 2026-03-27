using ThaumielMapEditor.API.Blocks.ServerObjects;

namespace ThaumielMapEditor.API.Enums
{
    /// <summary>
    /// Defines the types that <see cref="ClutterObject"/> can use.
    /// </summary>
    public enum CameraType
    {
        /// <summary>
        /// Light containment zone.
        /// </summary>
        Lcz = 0,

        /// <summary>
        /// Heavy containment zone.
        /// </summary>
        Hcz = 1,

        /// <summary>
        /// Entrance zone.
        /// </summary>
        Ez = 2,

        /// <summary>
        /// Entrance zone but with a arm.
        /// </summary>
        EzArm = 3,

        /// <summary>
        /// Surface zone
        /// </summary>
        Sz = 4
    }
}