using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.Areas
{
    public class CullingArea : AreaObject
    {
        public Vector3 Bounds { get; set; }

        private GameObject? _cullingObject;

        public void CreateCullingZone()
        {
            if (ParentSchematic == null)
                return;

            _cullingObject = new GameObject("CullingZone");
            _cullingObject.transform.position = ParentSchematic.Position;

            BoxCollider collider = _cullingObject.AddComponent<BoxCollider>();
            collider.size = Bounds;
            collider.isTrigger = true;

            CullingTrigger trigger = _cullingObject.AddComponent<CullingTrigger>();
            trigger.Init(ParentSchematic);
        }

        public void DestroyCullingZone()
        {
            if (_cullingObject == null)
                return;

            Object.Destroy(_cullingObject);
            _cullingObject = null;
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