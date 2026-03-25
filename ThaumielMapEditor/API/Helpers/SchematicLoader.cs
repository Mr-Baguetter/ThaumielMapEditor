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
        public static event Action<SchematicData>? SchematicSpawned;

        public static event Action<SchematicData>? SchematicDestroyed;

        public static Dictionary<Guid, MapData> MapsById { get; set; } = [];
        public static Dictionary<uint, SchematicData> SchematicsById { get; set; } = [];

        public static IEnumerable<SchematicData> SpawnedSchematics => SchematicsById.Values;
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

        public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .IgnoreFields()
            .WithTypeConverter(new CustomVectorConverter())
            .WithTypeConverter(new CustomColor32Converter())
            .WithTypeConverter(new CustomColorConverter())
            .WithTypeConverter(new CustomQuaternionConverter())
            .Build();

        public static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreFields()
            .WithTypeConverter(new CustomVectorConverter())
            .WithTypeConverter(new CustomColor32Converter())
            .WithTypeConverter(new CustomColorConverter())
            .WithTypeConverter(new CustomQuaternionConverter())
            .Build();

        public static void Init()
        {
            LoadSchematics();
            LoadMaps();
        }

        public static void ReloadSchematics()
        {
            LoadedSchematics.Clear();
            LoadSchematics();
        }

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

        public static SchematicData? GetSchematicById(uint id)
        {
            if (SchematicsById.TryGetValue(id, out var schem))
                return schem;

            return null;
        }

        public static uint GetId()
        {
            uint id = 0;
            while (SchematicsById.Keys.Contains(id))
                id++;

            return id;
        }

        private static void SpawnObjectRecursive(int id, SerializableSchematic schematic, SchematicData schematicData)
        {
            SerializableObject? obj = schematic.Objects.Find(o => o.ObjectId == id);
            if (obj != null)
            {
                SpawnSerializableObject(obj, schematicData);
            }

            SerializableArea? areaobj = schematic.Areas.Find(o => o.ObjectId == id);
            if (areaobj != null)
            {
                SpawnSerializableArea(areaobj, schematicData);
            }

            int[] nestedSchematicIds = schematic.Objects.Where(o => o.ObjectType == ObjectType.Schematic).Select(o => o.ObjectId).ToArray();
            foreach (SerializableObject objectChild in schematic.Objects.FindAll(o => o.ParentId == id))
            {
                if (nestedSchematicIds.Contains(objectChild.ParentId))
                    continue;

                SpawnObjectRecursive(objectChild.ObjectId, schematic, schematicData);
            }

            foreach (SerializableArea areaChild in schematic.Areas.FindAll(o => o.ParentId == id))
            {
                if (nestedSchematicIds.Contains(areaChild.ParentId))
                    continue;

                SpawnObjectRecursive(areaChild.ObjectId, schematic, schematicData);
            }
        }

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

            foreach (MapSchematic ms in map.Schematics)
            {
                Vector3 offset = data.Room.WorldPosition(ms.Position);
                SerializableSchematic? schematic = LoadedSchematics.FirstOrDefault(s => s.FileName.ToLower() == ms.SchematicName.ToLower());
                if (schematic == null)
                    continue;

                SchematicData schematicData = SpawnSchematic(schematic, offset);
                data.Schematics.Add((offset, schematicData.FileName));
            }

            MapsById.Add(data.Id, data);
        }

        public static SchematicData SpawnSchematic(SerializableSchematic schematic, Vector3 position)
        {
            SchematicData schematicData = new()
            {
                FileName = schematic.FileName,
                Id = GetId(),
                ContainsAnimator = schematic.ContainsAnimator
            };

            Timing.RunCoroutine(SpawnSchematicCoroutine(schematic, schematicData, position));
            return schematicData;
        }

        private static IEnumerator<float> SpawnSchematicCoroutine(SerializableSchematic schematic, SchematicData schematicData, Vector3 position)
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
            baseprimitive.Rotation = schematic.Rotation;
            baseprimitive.Scale = schematic.Scale;
            schematicData.Primitive = baseprimitive;
            schematicData.Scale = schematic.Scale;
            schematicData.Position = position;
            schematicData.Rotation = schematic.Rotation;
            schematicData.RootObjectId = schematic.RootObjectId;

            SpawnObjectRecursive(schematic.RootObjectId, schematic, schematicData);

            SchematicSpawned?.Invoke(schematicData);
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

                case ObjectType.Light:
                    LightObject light = new()
                    {
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic,
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        MovementSmoothing = serializable.MovementSmoothing,
                    };

                    light.SpawnObject(serializable, schematicData);
                    break;

                case ObjectType.TextToy:
                    TextToyObject textToy = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    textToy.SpawnObject(serializable, schematicData);
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

                case ObjectType.Capybara:
                    CapybaraObject capybara = new()
                    {
                        Position = serializable.Position,
                        Rotation = serializable.Rotation,
                        Scale = serializable.Scale,
                        IsStatic = serializable.IsStatic
                    };

                    capybara.Collisions = capybara.GetValue<bool>(serializable, "Collisions");
                    capybara.SpawnObject(schematicData, serializable);
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

                default:
                    LogManager.Warn($"Unhandled ObjectType '{serializable.ObjectType}' on object '{serializable.Name}', skipping.");
                    break;
            }
        }

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

        public static void GenerateExampleSchematic()
        {
            SerializableObject primitive = new()
            {
                Name = "Test",
                Scale = new(10, 10, 10),
                IsStatic = true,
                Position = new(0, 4, 0),
                Rotation = Quaternion.Euler(Vector3.zero),
                MovementSmoothing = 30,
                Values =
                {
                    ["Color"] = Color.red,
                    ["PrimitiveType"] = PrimitiveType.Cube,
                    ["PrimitiveFlags"] = PrimitiveFlags.Visible | PrimitiveFlags.Collidable
                },
            };

            SerializableSchematic schematic = new()
            {
                FileName = "example"
            };

            schematic.Objects.Add(primitive);
            string dir = ThaumFileManager.Dir(["Schematics"]);
            ThaumFileManager.TryCreateDirectory(dir);
            string path = Path.Combine(dir, "example.yml");
            if (!File.Exists(path))
                File.WriteAllText(path, Serializer.Serialize(schematic));
        }

        public static SerializableMap SaveMap(MapData data)
        {
            SerializableMap map = new();
            map.FileName = data.FileName;
            map.Room = data.Room.Name;
            map.Id = Guid.NewGuid();
            foreach ((Vector3, string) val in data.Schematics)
            {
                MapSchematic mapSchematic = new()
                {
                    Position = val.Item1,
                    SchematicName = val.Item2
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