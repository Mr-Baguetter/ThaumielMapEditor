// -----------------------------------------------------------------------
// <copyright file="CullingArea.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.Areas
{
    public class CullingArea : AreaObject
    {
        /// <summary>
        /// Gets or sets the bounds of the <see cref="CullingArea"/> instance.
        /// </summary>
        public Vector3 Bounds
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                if (CullingObject != null)
                {
                    BoxCollider collider = CullingObject.GetComponent<BoxCollider>();
                    collider?.size = value;
                }
            }
        }

        /// <summary>
        /// The <see cref="GameObject"/> instance tied to this <see cref="CullingArea"/> instance.
        /// </summary>
        public GameObject? CullingObject;

        /// <summary>
        /// Creates the CullingArea <see cref="GameObject"/> and adds the <see cref="CullingTrigger"/> component.
        /// </summary>
        public void CreateCullingZone()
        {
            if (ParentSchematic == null)
                return;

            CullingObject = new GameObject("CullingArea");
            CullingObject.transform.position = ParentSchematic.Position;

            BoxCollider collider = CullingObject.AddComponent<BoxCollider>();
            collider.size = Bounds;
            collider.isTrigger = true;

            CullingTrigger trigger = CullingObject.AddComponent<CullingTrigger>();
            trigger.Init(ParentSchematic);
        }

        /// <summary>
        /// Destroys this <see cref="CullingArea"/> instance.
        /// </summary>
        public void DestroyCullingArea()
        {
            if (CullingObject == null)
                return;

            Object.Destroy(CullingObject);
            CullingObject = null;
        }

        /// <inheritdoc/>
        public override void ParseValues()
        {
            if (!Values.TryConvertValue<Vector3>("Bounds", out var bounds))
            {
                LogManager.Warn($"Failed to parse Bounds");
                return;
            }

            Bounds = bounds;
        }
    }
}