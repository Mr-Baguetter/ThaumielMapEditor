using System.Collections.Generic;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Components.Tools
{
    public class ObjectPhysics : ToolBase
    {
        /// <inheritdoc/>
        public override ToolType Type => ToolType.Physics;

        /// <summary>
        /// Gets the <see cref="UnityEngine.Rigidbody"/> for this <see cref="ObjectPhysics"/> instance.
        /// Null until <see cref="SetupRigidbody()"/> is called by <see cref="Init(ServerObject, SchematicData, Dictionary{string, object})"/>.
        /// </summary>
        public Rigidbody? Rigidbody { get; private set; }

        /// <summary>
        /// Gets or sets the mass for the <see cref="Rigidbody"/>.
        /// </summary>
        public float Weight
        {
            get;
            set
            {
                Rigidbody?.mass = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the drag for the <see cref="Rigidbody"/>.
        /// </summary>
        public float Drag
        {
            get;
            set
            {
                Rigidbody?.linearDamping = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the rotation resistance for the <see cref="Rigidbody"/>.
        /// </summary>
        public float AngularDrag
        {
            get;
            set
            {
                Rigidbody?.angularDamping = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CollisionDetectionMode"/> for the <see cref="Rigidbody"/>.
        /// </summary>
        public CollisionDetectionMode CollisionMode
        {
            get;
            set
            {
                Rigidbody?.collisionDetectionMode = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="Rigidbody"/> applies physics or not
        /// </summary>
        public bool Enabled
        {
            get;
            set
            {
                Rigidbody?.isKinematic = !value;
                field = value;
            }
        }

        /// <inheritdoc/>
        public override void Init(ServerObject obj, SchematicData schem, Dictionary<string, object> properties)
        {
            Object = obj;
            Schematic = schem;

            if (!properties.TryConvertValue<float>("Weight", out var weight))
            {
                LogManager.Warn("Failed to parse Weight");
            }

            if (!properties.TryConvertValue<bool>("Enabled", out var enabled))
            {
                LogManager.Warn("Failed to parse Enabled");
            }

            if (!properties.TryConvertValue<float>("Drag", out var drag))
            {
                LogManager.Warn("Failed to parse Drag");
            }

            if (!properties.TryConvertValue<float>("AngularDrag", out var angularDrag))
            {
                LogManager.Warn("Failed to parse AngularDrag");
            }

            if (!properties.TryConvertValue<CollisionDetectionMode>("CollisionMode", out var collisionMode))
            {
                LogManager.Warn("Failed to parse CollisionMode");
            }

            Weight = weight;
            Enabled = enabled;
            Drag = drag;
            AngularDrag = angularDrag;
            CollisionMode = collisionMode;

            SetupRigidbody();
        }

        /// <summary>
        /// Adds the specified force to the <see cref="Rigidbody"/>
        /// </summary>
        /// <param name="force">The amount of <see cref="Vector3"/> force to add.</param>
        /// <param name="mode">The <see cref="ForceMode"/> that will be used.</param>
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Impulse)
        {
            if (Rigidbody == null || !Enabled)
                return;

            Rigidbody.AddForce(force, mode);
        }

        private void SetupRigidbody()
        {
            if (!TryGetComponent<Rigidbody>(out var body))
                body = gameObject.AddComponent<Rigidbody>();

            Rigidbody = body;
            Rigidbody.mass = Weight;
            Rigidbody.isKinematic = !Enabled;
            Rigidbody.linearDamping = Drag;
            Rigidbody.angularDamping = AngularDrag;
            Rigidbody.collisionDetectionMode = CollisionMode;
        }
    }
}