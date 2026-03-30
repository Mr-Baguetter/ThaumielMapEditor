using System.Collections.Generic;
using ThaumielMapEditor.API.Serialization;
using ThaumielMapEditor.API.Enums;
using System;
using System.Threading.Tasks;
using UnityEngine;
using AdminToys;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.API.Conversion
{
    public static class PMERConverter
    {
        /// <summary>
        /// Converts a PMER schematic into a TME schematic
        /// </summary>
        /// <param name="root">The PMER schematic root</param>
        /// <returns></returns>
        public static async Task<SerializableSchematic> ConvertSchematicAsync(PMERRoot root)
        {
            return await Task.Run(() =>
            {
                SerializableSchematic tme = new()
                {
                    FileName = "ConvertedSchematic",
                    Rotation = Quaternion.identity,
                    Scale = Vector3.one,
                    Objects = []
                };

                foreach (PMERBlock block in root.Blocks)
                    tme.Objects.Add(ConvertBlock(block));

                return tme;
            });
        }

        private static SerializableObject ConvertBlock(PMERBlock block)
        {
            SerializableObject obj = new()
            {
                Name = block.Name,
                Position = block.Position,
                Rotation = Quaternion.Euler(block.Rotation),
                Scale = block.Scale,
                IsStatic = block.Properties != null && block.Properties.TryGetValue("Static", out object s) && Convert.ToBoolean(s),
                MovementSmoothing = 60,
                ObjectType = MapBlockType(block.BlockType),
                Values = NormalizeProperties(block)
            };

            return obj;
        }

        private static ObjectType MapBlockType(int blockType)
        {
            return blockType switch
            {
                1 => ObjectType.Primitive,
                2 => ObjectType.Light,
                3 => ObjectType.Pickup,
                4 => ObjectType.Workstation,
                5 => ObjectType.Schematic,
                6 => ObjectType.Teleporter,
                7 => ObjectType.Locker,
                8 => ObjectType.TextToy,
                9 => ObjectType.Interactable,
                _ => throw new InvalidOperationException(),
            };
        }

        private static Dictionary<string, object> NormalizeProperties(PMERBlock block)
        {
            Dictionary<string, object> dict = block.Properties != null ? new Dictionary<string, object>(block.Properties) : [];

            if (dict.TryGetValue("PrimitiveType", out var pt))
                dict["PrimitiveType"] = Convert.ToInt32(pt);

            if (dict.TryGetValue("PrimitiveFlags", out var pf))
            {
                dict["PrimitiveFlags"] = Convert.ToByte(pf);
            }
            else
                dict["PrimitiveFlags"] = PrimitiveFlags.Visible | PrimitiveFlags.Collidable;

            if (dict.TryGetValue("Color", out var color))
            {
                if (TryParseHexColor(color.ToString(), out Color unityColor))
                {
                    dict["Color"] = unityColor;
                }
                else
                    LogManager.Warn($"Failed to parse color value: {color}");
            }

            return dict;
        }

        private static bool TryParseHexColor(string hex, out Color color)
        {
            color = Color.white;
            hex = hex.TrimStart('#');
            try
            {
                if (hex.Length == 6)
                {
                    color = new Color(
                        Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(4, 2), 16) / 255f
                    );

                    return true;
                }

                if (hex.Length == 8)
                {
                    color = new Color(
                        Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
                        Convert.ToInt32(hex.Substring(6, 2), 16) / 255f
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error($"Exception parsing hex color '{hex}': {ex.Message}");
            }

            return false;
        }
    }
}