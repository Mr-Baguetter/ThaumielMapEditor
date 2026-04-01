using UnityEngine;

namespace ThaumielMapEditor.API.Enums
{
    /// <summary>
    /// Defines the types of <see cref="Collider"/> as a enum.
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// <see cref="BoxCollider"/>
        /// </summary>
        Box,

        /// <summary>
        /// <see cref="SphereCollider"/>
        /// </summary>
        Sphere,

        /// <summary>
        /// <see cref="CapsuleCollider"/>
        /// </summary>
        Capsule,

        /// <summary>
        /// <see cref="MeshCollider"/>
        /// </summary>
        Mesh
    }
}
