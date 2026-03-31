using System.Collections.Generic;
using System.IO;
using AdminToys;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Yaml.CustomConverters;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ThaumielMapEditor.API.Data;
using System;
using Mirror;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using System.Linq;
using YamlDotNet.Core;
using Utils.NonAllocLINQ;
using ThaumielMapEditor.API.Blocks.Areas;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Blocks.ServerObjects.Lockers;
using ThaumielMapEditor.API.Extensions;

namespace ThaumielMapEditor.API.Helpers
{
    public class SchematicLoader
    {
        /// <summary>
        /// Fired when a <see cref="SchematicData"/> is spawned.
        /// </summary>
        public static event Action<SchematicData>? SchematicSpawned;

        /// <summary>
        /// Fired when a <see cref="SchematicData"/> is destroyed.
        /// </summary>
        public static event Action<SchematicData>? SchematicDestroyed;

        /// <summary>
        /// A dictionary of all spawned <see cref="MapData"/> instances, keyed by their <see cref="Guid"/>.
        /// </summary>
        public static Dictionary<Guid, MapData> MapsById { get; set; } = [];

        /// <summary>
        /// A dictionary of all spawned <see cref="SchematicData"/> instances, keyed by their ID.
        /// </summary>
        public static Dictionary<uint, SchematicData> SchematicsById { get; set; } = [];

        /// <summary>
        /// An enumerable of all currently spawned <see cref="SchematicData"/> instances.
        /// </summary>
        public static IEnumerable<SchematicData> SpawnedSchematics => SchematicsById.Values;

        /// <summary>
        /// An enumerable of all currently spawned <see cref="MapData"/> instances.
        /// </summary>
        public static IEnumerable<MapData> SpawnedMaps => MapsById.Values;

        /// <summary>
        /// This list contains all the schematics loaded by <see cref="LoadSchematics"/>
        /// Use <see cref="SpawnedSchematics"/> to get the spawned schematics.
        /// </summary>
        public static List<SerializableSchematic> LoadedSchematics = [];

        /// <summary>
        /// This list contains all the maps loaded by <see cref="LoadMaps"/>
        /// Use <see cref="SpawnedMaps"/> to get the spawned maps.
        /// </summary>
        public static List<SerializableMap> LoadedMaps = [];

        /// <summary>
        /// The YAML deserializer used to parse schematic and map files
        /// </summary>
        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .IgnoreFields()
            .WithTypeConverter(new CustomVectorConverter())
            .WithTypeConverter(new CustomColor32Converter())
            .WithTypeConverter(new CustomColorConverter())
            .WithTypeConverter(new CustomQuaternionConverter())
            .Build();

        /// <summary>
        /// The YAML serializer used to write schematic and map files, configured with Pascal case naming and custom type converters.
        /// </summary>
        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreFields()
            .WithTypeConverter(new CustomVectorConverter())
            .WithTypeConverter(new CustomColor32Converter())
            .WithTypeConverter(new CustomColorConverter())
            .WithTypeConverter(new CustomQuaternionConverter())
            .Build();

        /// <summary>
        /// Initializes the <see cref="SchematicLoader"/>
        /// </summary>
        public static void Init()
        {
            LoadSchematics();
            LoadMaps();
        }

        /// <summary>
        /// Reloads all loaded schematics
        /// </summary>
        /// <remarks>
        /// This does not automatically respawn schematics
        /// </remarks>
        public static void ReloadSchematics()
        {
            LoadedSchematics.Clear();
            LoadSchematics();
        }

        /// <summary>
        /// Destroys the specified <see cref="SchematicData"/>
        /// </summary>
        /// <param name="data">The schematic to be destroyed</param>
        public static void DestroySchematic(SchematicData data)
        {
            SchematicDestroyed?.Invoke(data);
            SchematicsById.Remove(data.Id);
            data.Destroy();
        }

        /// <summary>
        /// Loads the schematics
        /// </summary>
        public static void LoadSchematics()
        {
            string schematicDir = ThaumFileManager.Dir(["Schematics"]);
            ThaumFileManager.TryCreateDirectory(schematicDir);

            foreach (string filename in ThaumFileManager.GetFilesInDirectory(schematicDir))
            {
                try
                {
                    SerializableSchematic schematic = Deserializer.Deserialize<SerializableSchematic>(File.ReadAllText(filename));
                    schematic.FileName = Path.GetFileNameWithoutExtension(filename);
                    LoadedSchematics.Add(schematic);
                    LogManager.Debug($"Loaded schematic {Path.GetFileNameWithoutExtension(filename)}");
                }
                catch (YamlException yamlex)
                {
                    LogManager.Warn($"Failed to parse Schematic {Path.GetFileNameWithoutExtension(filename)}. \n\n {yamlex}");
                    continue;
                }
                catch (Exception ex)
                {
                    LogManager.Warn($"Exception when trying to parse Schematic {Path.GetFileNameWithoutExtension(filename)}. \n\n {ex}");
                    continue;
                }
            }
        }

        /// <summary>
        /// Loads the maps
        /// </summary>
        public static void LoadMaps()
        {
            string mapsDir = ThaumFileManager.Dir(["Maps"]);
            ThaumFileManager.TryCreateDirectory(mapsDir);

            foreach (string filename in ThaumFileManager.GetFilesInDirectory(mapsDir))
            {
                try
                {
                    SerializableMap map = Deserializer.Deserialize<SerializableMap>(File.ReadAllText(filename));
                    map.FileName = Path.GetFileNameWithoutExtension(filename);
                    LoadedMaps.Add(map);
                    LogManager.Debug($"Loaded map {Path.GetFileNameWithoutExtension(filename)}");
                }
                catch (YamlException yamlex)
                {
                    LogManager.Warn($"Failed to parse Schematic {Path.GetFileNameWithoutExtension(filename)}. \n\n {yamlex}");
                    continue;
                }
                catch (Exception ex)
                {
                    LogManager.Warn($"Exception when trying to parse Schematic {Path.GetFileNameWithoutExtension(filename)}. \n\n {ex}");
                    continue;
                }
            }
        }

        /// <summary>
        /// Tries to get the <see cref="SchematicData"/> by its Id.
        /// </summary>
        /// <param name="id">The id to get</param>
        /// <param name="schematic">The <see cref="SchematicData"/> if found</param>
        /// <returns><see langword="true"/> if found else returns <see langword="false"/> if not</returns>
        public static bool TryGetSchematicById(uint id, out SchematicData schematic)
        {
            SchematicData? data = GetSchematicById(id);
            if (data == null)
            {
                schematic = null;
                return false;
            }

            schematic = data;
            return true;
        }

        /// <summary>
        /// Gets the <see cref="SchematicData"/> by its id.
        /// </summary>
        /// <param name="id">The id to get</param>
        /// <returns><see cref="SchematicData"/> if found else returns <see langword="null"/></returns>
        public static SchematicData? GetSchematicById(uint id)
        {
            if (SchematicsById.TryGetValue(id, out var schem))
                return schem;

            return null;
        }

        /// <summary>
        /// Gets a unique id for all <see cref="SchematicData"/>
        /// </summary>
        /// <returns><see cref="uint"/> id</returns>
        public static uint GetId()
        {
            uint id = 0;
            while (SchematicsById.Keys.Contains(id))
                id++;

            return id;
        }

        private static void SpawnObjectRecursive(int id, SerializableSchematic schematic, SchematicData schematicData, HashSet<int>? visited = null)
        {
            visited ??= [];

            if (!visited.Add(id))
            {
                LogManager.Warn($"Cycle detected at ObjectId {id}, stopping recursion.");
                return;
            }

            SerializableObject? obj = schematic.Objects.Find(o => o.ObjectId == id);
            if (obj != null)
                SpawnSerializableObject(obj, schematicData);

            SerializableArea? areaobj = schematic.Areas.Find(o => o.ObjectId == id);
            if (areaobj != null)
                SpawnSerializableArea(areaobj, schematicData);

            int[] nestedSchematicIds = schematic.Objects.Where(o => o.ObjectType == ObjectType.Schematic).Select(o => o.ObjectId).ToArray();

            foreach (SerializableObject objectChild in schematic.Objects.FindAll(o => o.ParentId == id))
            {
                if (nestedSchematicIds.Contains(objectChild.ParentId))
                    continue;

                SpawnObjectRecursive(objectChild.ObjectId, schematic, schematicData, visited);
            }

            foreach (SerializableArea areaChild in schematic.Areas.FindAll(o => o.ParentId == id))
            {
                if (nestedSchematicIds.Contains(areaChild.ParentId))
                    continue;

                SpawnObjectRecursive(areaChild.ObjectId, schematic, schematicData, visited);
            }
        }

        /// <summary>
        /// Spawns a map
        /// </summary>
        /// <param name="map">The serialized map to spawn</param>
        /// <returns><see cref="MapData"/></returns>
        public static MapData SpawnMap(SerializableMap map)
        {
            MapData data = new()
            {
                Id = Guid.NewGuid(),
                Room = Room.List.Where(r => r.Name == map.Room).First(),
                Position = map.LocalPosition,
                FileName = map.FileName
            };

            Timing.RunCoroutine(SpawnMapCoroutine(map, data));
            return data;
        }

        private static IEnumerator<float> SpawnMapCoroutine(SerializableMap map, MapData data)
        {
            if (!PrefabHelper.RanRegister)
            {
                LogManager.Debug($"Waiting for prefabs to register before spawning map {map.FileName}...");
                yield return Timing.WaitUntilTrue(() => PrefabHelper.RanRegister);
            }

            foreach (SerializedMapSchematic ms in map.Schematics)
            {
                Vector3 offset = data.Room.WorldPosition(ms.Position);
                SerializableSchematic? schematic = LoadedSchematics.FirstOrDefault(s => s.FileName.ToLower() == ms.SchematicName.ToLower());
                if (schematic == null)
                    continue;

                SchematicData schematicData = SpawnSchematic(schematic, offset);
                data.Schematics.Add(new() { LocalPosition = offset, SchematicName = schematicData.FileName});
            }

            MapsById.Add(data.Id, data);
        }

        /// <summary>
        /// Spawns a schematic at the specified position, rotation, and scale.
        /// </summary>
        /// <param name="schematic">The serialized schematic to spawn.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <param name="rotation">The rotation to apply to the schematic.</param>
        /// <param name="scale">The scale to apply to the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(SerializableSchematic schematic, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            SchematicData schematicData = new()
            {
                FileName = schematic.FileName,
                Id = GetId(),
                ContainsAnimator = schematic.ContainsAnimator
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(schematic, schematicData, position, rotation, scale));
            return schematicData;
        }

        /// <summary>
        /// Spawns a schematic at the specified position and rotation with default scale.
        /// </summary>
        /// <param name="schematic">The serialized schematic to spawn.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <param name="rotation">The rotation to apply to the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(SerializableSchematic schematic, Vector3 position, Quaternion rotation)
        {
            SchematicData schematicData = new()
            {
                FileName = schematic.FileName,
                Id = GetId(),
                ContainsAnimator = schematic.ContainsAnimator
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(schematic, schematicData, position, rotation, default));
            return schematicData;
        }

        /// <summary>
        /// Spawns a schematic at the specified position with default rotation and scale.
        /// </summary>
        /// <param name="schematic">The serialized schematic to spawn.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(SerializableSchematic schematic, Vector3 position)
        {
            SchematicData schematicData = new()
            {
                FileName = schematic.FileName,
                Id = GetId(),
                ContainsAnimator = schematic.ContainsAnimator
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(schematic, schematicData, position, default, default));
            return schematicData;
        }
        
        /// <summary>
        /// Spawns a loaded schematic by name at the specified position with default rotation and scale.
        /// </summary>
        /// <param name="schematicname">The file name of the schematic to spawn. Must match a schematic in <see cref="LoadedSchematics"/>.</param>
        /// <param name="animated">Whether the schematic contains an animator.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(string schematicname, bool animated, Vector3 position)
        {
            SchematicData schematicData = new()
            {
                FileName = schematicname,
                Id = GetId(),
                ContainsAnimator = animated
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(LoadedSchematics.FirstOrDefault(s => s.FileName == schematicname), schematicData, position, default, default));
            return schematicData;
        }

        /// <summary>
        /// Spawns a loaded schematic by name at the specified position and rotation with default scale.
        /// </summary>
        /// <param name="schematicname">The file name of the schematic to spawn. Must match a schematic in <see cref="LoadedSchematics"/>.</param>
        /// <param name="animated">Whether the schematic contains an animator.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <param name="rotation">The rotation to apply to the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(string schematicname, bool animated, Vector3 position, Quaternion rotation)
        {
            SchematicData schematicData = new()
            {
                FileName = schematicname,
                Id = GetId(),
                ContainsAnimator = animated
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(LoadedSchematics.FirstOrDefault(s => s.FileName == schematicname), schematicData, position, rotation, default));
            return schematicData;
        }

        /// <summary>
        /// Spawns a loaded schematic by name at the specified position, rotation, and scale.
        /// </summary>
        /// <param name="schematicname">The file name of the schematic to spawn. Must match a schematic in <see cref="LoadedSchematics"/>.</param>
        /// <param name="animated">Whether the schematic contains an animator.</param>
        /// <param name="position">The world position at which to place the schematic.</param>
        /// <param name="rotation">The rotation to apply to the schematic.</param>
        /// <param name="scale">The scale to apply to the schematic.</param>
        /// <returns>A <see cref="SchematicData"/> instance representing the spawned schematic.</returns>
        public static SchematicData SpawnSchematic(string schematicname, bool animated, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            SchematicData schematicData = new()
            {
                FileName = schematicname,
                Id = GetId(),
                ContainsAnimator = animated
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(LoadedSchematics.FirstOrDefault(s => s.FileName == schematicname), schematicData, position, rotation, scale));
            return schematicData;
        }

        private static IEnumerator<float> SpawnSchematicCoroutine(SerializableSchematic schematic, SchematicData schematicData, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!PrefabHelper.RanRegister)
            {
                LogManager.Debug($"Waiting for prefabs to register before spawning schematic '{schematic.FileName}'...");
                yield return Timing.WaitUntilTrue(() => PrefabHelper.RanRegister);
            }

            LabPrimitive baseprimitive = LabPrimitive.Create();
            baseprimitive.Type = PrimitiveType.Cube;
            baseprimitive.Flags &= ~PrimitiveFlags.Visible;
            baseprimitive.Flags &= ~PrimitiveFlags.Collidable;
            baseprimitive.Position = position;
            if (rotation != default)
            {
                baseprimitive.Rotation = rotation;
                schematicData.Rotation = rotation;
            }
            else
            {
                baseprimitive.Rotation = schematic.Rotation;
                schematicData.Rotation = schematic.Rotation;
            }

            if (scale != default)
            {
                baseprimitive.Scale = scale;
                schematicData.Scale = scale;
            }
            else
            {
                baseprimitive.Scale = schematic.Scale;
                schematicData.Scale = schematic.Scale;
            }

            schematicData.Primitive = baseprimitive;
            schematicData.Position = position;
            schematicData.RootObjectId = schematic.RootObjectId;

            SpawnObjectRecursive(schematic.RootObjectId, schematic, schematicData);

            SchematicSpawned?.Invoke(schematicData);
            LODHelper.GenerateLODZones(schematicData);
            LogManager.Info($"Schematic '{schematic.FileName}' fully spawned.");
            SchematicsById.Add(schematicData.Id, schematicData);
        }

        private static void SpawnSerializableArea(SerializableArea area, SchematicData schematic)
        {
            switch (area.AreaType)
            {
                case AreaType.CullingArea:
                    CullingArea culling = new();
                    culling.ParentSchematic = schematic;
                    culling.ParseValues();
                    culling.CreateCullingZone();
                    schematic.SpawnedAreas.Add(culling);
                    break;
            }
        }

        private static void SpawnSerializableObject(SerializableObject serializable, SchematicData schematicData)
        {
            switch (serializable.ObjectType)
            {
                case ObjectType.Primitive:
                    PrimitiveObject primitive = new()
                    {
                        Name = serializable.Name,
                        ParentId = schematicData.Primitive!.Base.netId,
                        NetId = NetworkIdentity.GetNextNetworkId(),
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic,
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        MovementSmoothing = serializable.MovementSmoothing,
                        AssetId = PrefabHelper.PrimitiveAssetId,
                        Schematic = schematicData
                    };

                    primitive.DeserializeValues(serializable);
                    schematicData.SpawnedClientObjects.Add(primitive);

                    foreach (Player player in Player.ReadyList)
                        primitive.SpawnForPlayer(player);

                    CreateCollisionMesh(primitive);
                    break;

                case ObjectType.Capybara:
                    CapybaraObject capybara = new()
                    {
                        Name = serializable.Name,
                        ParentId = schematicData.Primitive!.Base.netId,
                        NetId = NetworkIdentity.GetNextNetworkId(),
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic,
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        MovementSmoothing = serializable.MovementSmoothing,
                        Schematic = schematicData
                    };

                    capybara.CollisionsEnabled = capybara.GetValue<bool>(serializable, "Collisions");
                    schematicData.SpawnedClientObjects.Add(capybara);

                    foreach (Player player in Player.ReadyList)
                        capybara.SpawnForPlayer(player);

                    if (capybara.CollisionsEnabled)
                        ColliderHelper.CreateClientObjectColliders(capybara, schematicData);
                        
                    break;

                case ObjectType.Light:
                    LightObject light = new()
                    {
                        ParentId = schematicData.Primitive!.Base.netId,
                        NetId = NetworkIdentity.GetNextNetworkId(),
                        AssetId = PrefabHelper.LightSource.netIdentity.assetId,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic,
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        MovementSmoothing = serializable.MovementSmoothing,
                        Schematic = schematicData
                    };

                    light.DeserializeValues(serializable);
                    schematicData.SpawnedClientObjects.Add(light);

                    foreach (Player player in Player.ReadyList)
                    {
                        light.SpawnForPlayer(player);                        
                    }
                        
                    break;

                case ObjectType.Clutter:
                    ClutterObject clutter = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    clutter.Type = clutter.GetValue<ClutterType>(serializable, "ClutterType");
                    clutter.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Door:
                    DoorObject door = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    door.ParseValues(serializable);
                    door.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.TextToy:
                    TextToyObject textToy = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    textToy.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Workstation:
                    WorkstationObject workstation = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    workstation.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Camera:
                    CameraObject camera = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    camera.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Interactable:
                    InteractionObject interaction = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    interaction.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Waypoint:
                    WaypointObject waypoint = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    waypoint.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Locker:
                    LockerObject locker = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    locker.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Pickup:
                    PickupObject pickup = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    pickup.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Target:
                    TargetDummyObject target = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    target.SpawnObject(schematicData, serializable);
                    break;

                case ObjectType.Teleporter:
                    TeleporterObject teleporter = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    teleporter.SpawnObject(schematicData, serializable);
                    break;

                default:
                    LogManager.Warn($"Unhandled ObjectType '{serializable.ObjectType}' on object '{serializable.Name}', skipping.");
                    break;
            }
        }

        /// <summary>
        /// Creates the colliders for primitives
        /// </summary>
        /// <param name="primitive"></param>
        public static void CreateCollisionMesh(PrimitiveObject primitive)
        {
            if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
                return;

            GameObject collider = new();
            collider.transform.localScale = new(Math.Abs(primitive.Scale.x), Math.Abs(primitive.Scale.y), Math.Abs(primitive.Scale.z));
            collider.transform.rotation = primitive.Rotation;
            collider.transform.position = primitive.Schematic.Primitive.Transform.TransformPoint(primitive.Position);
            collider.transform.name = $"[ThaumielMapEditor] {primitive.Name}";

            MeshCollider mesh = collider.AddComponent<MeshCollider>();
            mesh.sharedMesh = AdminToys.PrimitiveObjectToy.PrimitiveTypeToMesh[primitive.PrimitiveType];
            if (mesh != null)
            {
                primitive.ServerCollider = mesh;
                collider.transform.SetParent(primitive.Schematic.Primitive.Transform, true);
            }
            else
            {
                LogManager.Warn($"Failed to get mesh for primitive {primitive.Name}, skipping collider.");
                UnityEngine.Object.Destroy(collider);
            }
        }

        /// <summary>
        /// Saves a map
        /// </summary>
        /// <param name="data">The <see cref="MapData"/> to save</param>
        /// <returns><see cref="SerializableMap"/></returns>
        public static SerializableMap SaveMap(MapData data)
        {
            SerializableMap map = new();
            map.FileName = data.FileName;
            map.Room = data.Room.Name;
            map.Id = Guid.NewGuid();
            foreach (MapSchematicData msdata in data.Schematics)
            {
                SerializedMapSchematic mapSchematic = new()
                {
                    Position = msdata.LocalPosition,
                    SchematicName = msdata.SchematicName
                };

                map.Schematics.Add(mapSchematic);
            }

            string mapsDir = ThaumFileManager.Dir(["Maps"]);
            ThaumFileManager.TryCreateDirectory(mapsDir);
            File.WriteAllText(Path.Combine(mapsDir, $"{map.FileName}.yml"), Serializer.Serialize(map));
            return map;
        }
    }
}