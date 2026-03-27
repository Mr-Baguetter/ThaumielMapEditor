using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class ColliderHelper
    {
        public static void CreateClientObjectColliders(ClientSideObjectBase clientObject, SchematicData schematic)
        {
            GameObject? prefab = GetPrefabForClientObject(clientObject);

            if (prefab == null)
            {
                LogManager.Warn($"Could not find prefab for client object of type '{clientObject.ObjectType}', skipping colliders.");
                return;
            }

            Collider[] prefabColliders = prefab.GetComponentsInChildren<Collider>(includeInactive: true);
            if (prefabColliders.Length == 0)
            {
                LogManager.Debug($"No colliders found on prefab '{prefab.name}', skipping.");
                return;
            }

            foreach (Collider col in prefabColliders)
            {
                if (!col.enabled)
                    continue;

                Vector3 localOffset = col.transform.localPosition;
                Quaternion localRotation = col.transform.localRotation;
                Vector3 localScale = col.transform.localScale;

                GameObject colGo = new($"[ThaumielMapEditor] Collider_{clientObject.ObjectType}_{col.GetType().Name}");

                colGo.transform.position = clientObject.Position + clientObject.Rotation * Vector3.Scale(localOffset, clientObject.Scale);
                colGo.transform.rotation = clientObject.Rotation * localRotation;
                colGo.transform.localScale = Vector3.Scale(clientObject.Scale, localScale);

                if (TryCopyCollider(col, colGo))
                {
                    colGo.transform.SetParent(schematic.Primitive.Transform, worldPositionStays: true);
                    LogManager.Debug($"Created {col.GetType().Name} for '{clientObject.ObjectType}' at {colGo.transform.position}.");
                }
                else
                {
                    LogManager.Warn($"Unhandled collider type '{col.GetType().Name}' on prefab '{prefab.name}', skipping.");
                    UnityEngine.Object.Destroy(colGo);
                }
            }
        }

        /// <summary>
        /// Copies a <see cref="Collider"/> component from a prefab onto the target <see cref="GameObject"/>.
        /// Returns <see langword="true"/> if the collider type was handled.
        /// </summary>
        private static bool TryCopyCollider(Collider source, GameObject target)
        {
            switch (source)
            {
                case BoxCollider box:
                    BoxCollider newBox = target.AddComponent<BoxCollider>();
                    newBox.center = box.center;
                    newBox.size = box.size;
                    newBox.isTrigger = box.isTrigger;
                    return true;

                case SphereCollider sphere:
                    SphereCollider newSphere = target.AddComponent<SphereCollider>();
                    newSphere.center = sphere.center;
                    newSphere.radius = sphere.radius;
                    newSphere.isTrigger = sphere.isTrigger;
                    return true;

                case CapsuleCollider capsule:
                    CapsuleCollider newCapsule = target.AddComponent<CapsuleCollider>();
                    newCapsule.center = capsule.center;
                    newCapsule.radius = capsule.radius;
                    newCapsule.height = capsule.height;
                    newCapsule.direction = capsule.direction;
                    newCapsule.isTrigger = capsule.isTrigger;
                    return true;

                case MeshCollider mesh:
                    MeshCollider newMesh = target.AddComponent<MeshCollider>();
                    newMesh.sharedMesh = mesh.sharedMesh;
                    newMesh.convex = mesh.convex;
                    newMesh.isTrigger = mesh.isTrigger;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Resolves the source prefab <see cref="GameObject"/> for a given client-side object,
        /// used to extract its collider hierarchy.
        /// </summary>
        private static GameObject? GetPrefabForClientObject(ClientSideObjectBase clientObject)
        {
            return clientObject.ObjectType switch
            {
                ObjectType.Primitive => PrefabHelper.PrimitiveObject?.gameObject,
                ObjectType.Capybara => PrefabHelper.Capybara?.gameObject,
                _ => null
            };
        }
    }
}