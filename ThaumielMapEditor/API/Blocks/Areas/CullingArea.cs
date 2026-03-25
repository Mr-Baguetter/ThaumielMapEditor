using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.Areas
{
    public class CullingArea : AreaObject
    {
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

        public GameObject? CullingObject;

        public void CreateCullingZone()
        {
            if (ParentSchematic == null)
                return;

            CullingObject = new GameObject("CullingZone");
            CullingObject.transform.position = ParentSchematic.Position;

            BoxCollider collider = CullingObject.AddComponent<BoxCollider>();
            collider.size = Bounds;
            collider.isTrigger = true;

            CullingTrigger trigger = CullingObject.AddComponent<CullingTrigger>();
            trigger.Init(ParentSchematic);
        }

        public void DestroyCullingZone()
        {
            if (CullingObject == null)
                return;

            Object.Destroy(CullingObject);
            CullingObject = null;
        }

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