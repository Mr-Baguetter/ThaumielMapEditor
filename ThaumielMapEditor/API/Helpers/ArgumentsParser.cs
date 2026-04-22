// -----------------------------------------------------------------------
// <copyright file="ArgumentsParser.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Discord;
using ThaumielMapEditor.API.Serialization;
using ThaumielMapEditor.API.Helpers.BlockParser;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System;
using PlayerRoles;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using System.Linq;

namespace ThaumielMapEditor.API.Helpers
{
    public class ArgumentsParser
    {
        public static Dictionary<BlockyPayload, List<object>> Cache { get; set; } = [];

        public static IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static List<object> Load(BlockyPayload payload)
        {
            if (payload.Language != "yaml")
                return [];

            if (Cache.TryGetValue(payload, out var blocks))
                return blocks;

            List<object> parsedBlocks = [];
            List<Dictionary<string, object>> values = Deserializer.Deserialize<List<Dictionary<string, object>>>(payload.Code);
            if (values == null)
                return parsedBlocks;

            foreach (Dictionary<string, object> dict in values)
            {
                object? parsedBlock = ParseBlock(dict);
                if (parsedBlock != null)
                {
                    parsedBlocks.Add(parsedBlock);
                }
            }

            Cache.Add(payload, parsedBlocks);
            return parsedBlocks;
        }

        internal static object? ParseBlock(Dictionary<string, object> dict)
        {
            if (!dict.TryGetValue("type", out object? typeObj))
                return null;

            string type = typeObj.ToString();
            return type switch
            {
                "math_number" => new MathNumberBlock
                {
                    Num = ParseFloat(dict, "NUM")
                },

                "text" => new TextBlock
                {
                    Text = GetString(dict, "TEXT")
                },

                "logic_boolean" => new LogicBooleanBlock
                {
                    Value = ParseBool(dict, "BOOL")
                },

                "math_arithmetic" => new MathArithmeticBlock
                {
                    OP = GetString(dict, "OP"),
                    A  = ParseValue(dict.GetValueOrDefault("A")),
                    B  = ParseValue(dict.GetValueOrDefault("B"))
                },
                
                "math_single" => new MathSingleBlock
                {
                    OP  = GetString(dict, "OP"),
                    NUM = ParseValue(dict.GetValueOrDefault("NUM"))
                },
                
                "math_trig" => new MathTrigBlock
                {
                    OP  = GetString(dict, "OP"),
                    NUM = ParseValue(dict.GetValueOrDefault("NUM"))
                },
                
                "math_round" => new MathRoundBlock
                {
                    OP  = GetString(dict, "OP"),
                    NUM = ParseValue(dict.GetValueOrDefault("NUM"))
                },
                
                "math_modulo" => new MathModuloBlock
                {
                    DIVIDEND = ParseValue(dict.GetValueOrDefault("DIVIDEND")),
                    DIVISOR  = ParseValue(dict.GetValueOrDefault("DIVISOR"))
                },
                
                "math_constrain" => new MathConstrainBlock
                {
                    VALUE = ParseValue(dict.GetValueOrDefault("VALUE")),
                    LOW   = ParseValue(dict.GetValueOrDefault("LOW")),
                    HIGH  = ParseValue(dict.GetValueOrDefault("HIGH"))
                },
                
                "math_random_float" => new MathRandomFloatBlock(),

                "timing_wait_for_frames" => new WaitForFrames
                {
                    WaitTime = (uint)ParseFloat(dict, "WaitTime")
                },

                "timing_wait_for_seconds" => new WaitForSeconds
                {
                    WaitTime = (uint)ParseFloat(dict, "WaitTime")
                },

                "timing_wait_until_true" => new WaitUntilBlock
                {
                    Condition = ParseBlockBase(dict),
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "procedures_callnoreturn" => new ProcedureCallNoReturnBlock
                {
                    Name = GetString(dict, "NAME")
                },

                "procedures_callreturn" => new ProcedureCallReturnBlock
                {
                    Name = GetString(dict, "NAME"),
                    Args = ParseCallArgs(dict)
                },

                "procedures_defreturn" => new MethodBlock
                {
                    Type = type,
                    Name = dict.TryGetValue("NAME", out object? drName) ? drName.ToString() : string.Empty,
                    Stack = dict.TryGetValue("STACK", out object? drStack) ? ParseStatementList(drStack).Select(ParseBlock).Where(x => x != null).ToList()! : [],
                    Params = dict.TryGetValue("PARAMS", out var drp) ? ParseParams(drp) : [],
                    Return = dict.TryGetValue("RETURN", out object? ret) ? ParseBlock((Dictionary<string, object>)ret) : null
                },

                "spawned_event" => new EventBlock
                {
                    EventType = EventType.OnSpawned,
                    ParamName = "ignore",
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "trigger_enter_event" => new EventBlock
                {
                    EventType = EventType.OnTriggerEntered,
                    ParamName = "player",
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "trigger_exit_event" => new EventBlock
                {
                    EventType = EventType.OnTriggerExited,
                    ParamName = "player",
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "interaction_event" => new EventBlock
                {
                    EventType = EventType.OnInteraction,
                    ParamName = "player",
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "interaction_denied_event" => new EventBlock
                {
                    EventType = EventType.OnInteractionDenied,
                    ParamName = "player",
                    Stack = dict.TryGetValue("DO", out object? evtStack) ? ParseStatementList(evtStack).Select(ParseBlock).Where(x => x != null).ToList()! : []
                },

                "procedures_defnoreturn" or "procedures_defreturn" => new MethodBlock
                {
                    Type = type,
                    Name = dict.TryGetValue("NAME", out object? name) ? name.ToString() : string.Empty,
                    Stack = dict.TryGetValue("STACK", out object? stack) ? ParseStatementList(stack).Select(ParseBlock).Where(x => x != null).ToList() : [],
                    Params = dict.TryGetValue("PARAMS", out var p) ? ParseParams(p) : []
                },

                "primitive_create" => new PrimitiveCreateBlock
                {
                    Name = GetString(dict, "name"),
                    IsServerSide = ParseBool(dict, "isServerSide")
                },

                "primitive_set_position" or
                "primitive_set_rotation" or
                "primitive_set_scale" => new PrimitiveVectorBlock
                {
                    TargetProperty = type.Replace("primitive_set_", ""),
                    X = ParseFloat(dict, "x"),
                    Y = ParseFloat(dict, "y"),
                    Z = ParseFloat(dict, "z")
                },

                "primitive_set_color" => new PrimitiveColorBlock
                {
                    R = ParseFloat(dict, "r", 1f),
                    G = ParseFloat(dict, "g", 1f),
                    B = ParseFloat(dict, "b", 1f),
                    A = ParseFloat(dict, "a", 1f)
                },

                "primitive_set_type" => new PrimitiveStateBlock { SettingType = "type", StringValue = GetString(dict, "primitiveType") },
                "primitive_set_flags" => new PrimitiveStateBlock { SettingType = "flags", StringValue = GetString(dict, "primitiveFlags") },
                "get_primitive_property" => new PrimitiveStateBlock { SettingType = "property", StringValue = GetString(dict, "Property") },
                "primitive_set_static" => new PrimitiveStateBlock { SettingType = "static", BoolValue = ParseBool(dict, "isStatic") },
                "primitive_set_smoothing" => new PrimitiveStateBlock { SettingType = "smoothing", ByteValue = (byte)ParseFloat(dict, "movementSmoothing") },

                "logger_log" => new LoggingBlock
                {
                    Level = GetLogLevel(GetString(dict, "LEVEL")),
                    Text = GetString(dict, "M")
                },

                "variables_set" => new VariableBlock
                {
                    Name = dict.TryGetValue("VAR", out object? varName) ? varName.ToString() : string.Empty,
                    Value = dict.TryGetValue("VALUE", out object? val) ? val : null
                },

                "variables_get" => new GetVariableBlock
                {
                    Name = dict.TryGetValue("VAR", out object? varName) ? varName.ToString() : string.Empty,
                },

                "get_player_property" => new PlayerGetPropertyBlock
                {
                    Property = GetString(dict, "Property")
                },

                "set_player_gravity" => new PlayerSetGravityBlock
                {
                    X = ParseFloat(dict, "x"),
                    Y = ParseFloat(dict, "y", -19.86f),
                    Z = ParseFloat(dict, "z")
                },

                "get_player_by_id" => new PlayerGetByIdBlock
                {
                    PlayerId = (int)ParseFloat(dict, "Player Id")
                },

                "get_player_by_userid" => new PlayerGetByUserIdBlock
                {
                    UserId = GetString(dict, "Player User Id")
                },

                "set_player_role" => new PlayerSetRoleBlock
                {
                    Role = Enum.TryParse(GetString(dict, "New Role"), out RoleTypeId role) ? role : RoleTypeId.None,
                    KeepPosition = ParseBool(dict, "Keep Position")
                },

                "give_player_item" => new PlayerGiveItemBlock
                {
                    Item = Enum.TryParse(GetString(dict, "New Item"), out ItemType item) ? item : ItemType.None,
                    Amount = (int)ParseFloat(dict, "Amount", 1f),
                    DropIfFull = ParseBool(dict, "Drop if full")
                },

                "remove_player_item" => new PlayerRemoveItemBlock
                {
                    Item = Enum.TryParse(GetString(dict, "Removed Item"), out ItemType removedItem) ? removedItem : ItemType.None
                },

                "set_player_health" => new PlayerSetHealthBlock { HealthType = "health", Value = ParseFloat(dict, "Health") },
                "set_player_max_health" => new PlayerSetHealthBlock { HealthType = "max_health", Value = ParseFloat(dict, "Max Health") },
                "set_player_artificial_health" => new PlayerSetHealthBlock { HealthType = "artificial_health", Value = ParseFloat(dict, "Artificial Health") },
                "set_player_max_artificial_health" => new PlayerSetHealthBlock { HealthType = "max_artificial_health", Value = ParseFloat(dict, "Max Artificial Health") },
                "set_player_hume_shield" => new PlayerSetHealthBlock { HealthType = "hume_shield", Value = ParseFloat(dict, "Hume shield") },
                "set_player_max_hume_shield" => new PlayerSetHealthBlock { HealthType = "max_hume_shield", Value = ParseFloat(dict, "Max Hume Shield") },
                "set_player_hume_shield_regen_rate" => new PlayerSetHealthBlock { HealthType = "hume_shield_regen_rate", Value = ParseFloat(dict, "Hume Shield Regen Rate") },
                "set_player_hume_shield_regen_cooldown" => new PlayerSetHealthBlock { HealthType = "hume_shield_regen_cooldown", Value = ParseFloat(dict, "Hume Shield Regen Cooldown") },

                "set_player_group_name" => new PlayerSetGroupBlock { GroupType = "name", Value = GetString(dict, "Group Name") },
                "set_player_group_color" => new PlayerSetGroupBlock { GroupType = "color", Value = GetString(dict, "Group Color") },

                "send_player_broadcast" => new PlayerSendBroadcastBlock
                {
                    Message = GetString(dict, "Broadcast Message"),
                    Duration = (ushort)ParseFloat(dict, "Duration", 5f)
                },

                "send_player_hint" => new PlayerSendHintBlock
                {
                    Message = GetString(dict, "Hint Message"),
                    Duration = (ushort)ParseFloat(dict, "Duration", 5f)
                },

                "set_player_scale" => new PlayerSetScaleBlock
                {
                    X = ParseFloat(dict, "x"),
                    Y = ParseFloat(dict, "y"),
                    Z = ParseFloat(dict, "z")
                },

                "texttoy_create" => new TextToyCreateBlock
                {
                    Name = GetString(dict, "name")
                },

                "texttoy_set_text" => new TextToySetTextBlock
                {
                    Text = GetString(dict, "text")
                },

                "texttoy_set_display_size" => new TextToySetDisplaySizeBlock
                {
                    X = ParseFloat(dict, "x", 1f),
                    Y = ParseFloat(dict, "y", 1f)
                },

                "get_texttoy_property" => new TextToyGetPropertyBlock
                {
                    Property = GetString(dict, "Property")
                },

                "waypoint_create" => new WaypointCreateBlock
                {
                    Name = GetString(dict, "name")
                },

                "waypoint_set_visualize_bounds" => new WaypointSetVisualizeBoundsBlock
                {
                    VisualizeBounds = ParseBool(dict, "visualizeBounds")
                },

                "waypoint_set_priority" => new WaypointSetPriorityBlock
                {
                    Priority = ParseFloat(dict, "priority")
                },

                "waypoint_set_bounds_size" => new WaypointSetBoundsSizeBlock
                {
                    X = ParseFloat(dict, "x", 1f),
                    Y = ParseFloat(dict, "y", 1f),
                    Z = ParseFloat(dict, "z", 1f)
                },

                "get_waypoint_property" => new WaypointGetPropertyBlock
                {
                    Property = GetString(dict, "Property")
                },

                "speaker_create" => new SpeakerCreateBlock
                {
                    Name = GetString(dict, "name")
                },

                "speaker_set_volume" => new SpeakerSetVolumeBlock { Volume = ParseFloat(dict, "volume", 100f) },
                "speaker_set_is_spatial" => new SpeakerSetIsSpatialBlock { IsSpatial = ParseBool(dict, "isSpatial") },
                "speaker_set_min_distance" => new SpeakerSetMinDistanceBlock { MinDistance = ParseFloat(dict, "minDistance", 1f) },
                "speaker_set_max_distance" => new SpeakerSetMaxDistanceBlock { MaxDistance = ParseFloat(dict, "maxDistance", 10f) },
                "speaker_set_loop" => new SpeakerSetLoopBlock { Loop = ParseBool(dict, "loop") },
                "speaker_set_path" => new SpeakerSetPathBlock { Path = GetString(dict, "path") },
                "speaker_pause" => new SpeakerPauseBlock(),
                "speaker_unpause" => new SpeakerUnpauseBlock(),

                "speaker_play" => new SpeakerPlayBlock
                {
                    FilePath = GetString(dict, "filepath")
                },

                "get_speaker_property" => new SpeakerGetPropertyBlock
                {
                    Property = GetString(dict, "Property")
                },

                "run_method" => new RunMethodBlock
                {
                    FullMethodName = GetString(dict, "Full Method Name (namespace + method)"),
                    Args =
                    [
                        dict.TryGetValue("Argument 1", out object? a1) ? a1 : null,
                        dict.TryGetValue("Argument 2", out object? a2) ? a2 : null,
                        dict.TryGetValue("Argument 3", out object? a3) ? a3 : null,
                        dict.TryGetValue("Argument 4", out object? a4) ? a4 : null,
                    ]
                },

                "run_method_instance" => new RunMethodInstanceBlock
                {
                    Instance = dict.TryGetValue("Instance", out object? inst) ? inst : null,
                    FullMethodName = GetString(dict, "Full Method Name (namespace + method)"),
                    Args =
                    [
                        dict.TryGetValue("Argument 1", out object? ra1) ? ra1 : null,
                        dict.TryGetValue("Argument 2", out object? ra2) ? ra2 : null,
                        dict.TryGetValue("Argument 3", out object? ra3) ? ra3 : null,
                        dict.TryGetValue("Argument 4", out object? ra4) ? ra4 : null,
                    ]
                },

                "play_animation" => new PlayAnimationBlock
                {
                    AnimationName = GetString(dict, "animationName")
                },

                "play_audio" => new PlayAudioBlock
                {
                    Path = GetString(dict, "path"),
                    Volume = ParseFloat(dict, "volume", 1f),
                    MinDistance = ParseFloat(dict, "minDistance", 1f),
                    MaxDistance = ParseFloat(dict, "maxDistance", 20f),
                    IsSpatial = ParseBool(dict, "isSpatial")
                },

                "send_cassie" => new SendCassieBlock
                {
                    Message = GetString(dict, "message"),
                    CustomSubtitles = GetString(dict, "customSubtitles"),
                    PlayBackground = ParseBool(dict, "playBackground"),
                    Priority = ParseFloat(dict, "priority"),
                    GlitchScale = ParseFloat(dict, "glitchScale")
                },

                "run_command" => new RunCommandBlock
                {
                    CommandType = GetString(dict, "commandType"),
                    Command = GetString(dict, "command")
                },

                "give_effect" => new GiveEffectBlock
                {
                    Effect = Enum.TryParse(GetString(dict, "effect"), out EffectType et) ? et : default,
                    Intensity = (byte)ParseFloat(dict, "intensity", 1f),
                    Duration = ParseFloat(dict, "duration", 5f)
                },

                "remove_effect" => new RemoveEffectBlock
                {
                    Effect = Enum.TryParse(GetString(dict, "effect"), out EffectType ret) ? ret : default,
                    Intensity = (byte)ParseFloat(dict, "intensity", 1f)
                },

                "give_item" => new ActionGiveItemBlock
                {
                    Item = Enum.TryParse(GetString(dict, "item"), out ItemType git) ? git : ItemType.None,
                    Count = (int)ParseFloat(dict, "count", 1f)
                },

                "remove_item" => new ActionRemoveItemBlock
                {
                    Item = Enum.TryParse(GetString(dict, "item"), out ItemType rit) ? rit : ItemType.None,
                    Count = (int)ParseFloat(dict, "count", 1f)
                },

                "warhead" => new WarheadBlock
                {
                    Action = Enum.TryParse(GetString(dict, "action"), out WarheadAction wa) ? wa : default,
                    SuppressSubtitles = ParseBool(dict, "suppressSubtitles")
                },

                "unity_get_pos" => new UnityGetPosBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!
                },

                "unity_set_pos" => new UnitySetPosBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!,
                    POS = ParseVector3(dict.GetValueOrDefault("POS"))!
                },

                "unity_translate" => new UnityTranslateBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!,
                    V = ParseVector3(dict.GetValueOrDefault("V"))
                },

                "unity_rotate" => new UnityRotateBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!,
                    V = ParseVector3(dict.GetValueOrDefault("V"))
                },

                "unity_set_scale" => new UnitySetScaleBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!,
                    S = ParseVector3(dict.GetValueOrDefault("S"))
                },

                "unity_look_at" => new UnityLookAtBlock
                {
                    OBJ = ParseValue(dict.GetValueOrDefault("OBJ"))!,
                    T = ParseValue(dict.GetValueOrDefault("T"))!
                },

                "unity_add_force" => new UnityAddForceBlock
                {
                    RB = ParseValue(dict.GetValueOrDefault("RB"))!,
                    F = ParseValue(dict.GetValueOrDefault("F"))
                },

                "unity_add_torque" => new UnityAddTorqueBlock
                {
                    RB = ParseValue(dict.GetValueOrDefault("RB"))!,
                    T = ParseValue(dict.GetValueOrDefault("T"))
                },

                "unity_set_vel" => new UnitySetVelBlock
                {
                    RB = ParseValue(dict.GetValueOrDefault("RB"))!,
                    V = ParseValue(dict.GetValueOrDefault("V"))
                },

                "unity_raycast" => new UnityRaycastBlock
                {
                    O = ParseValue(dict.GetValueOrDefault("O")),
                    D = ParseValue(dict.GetValueOrDefault("D"))
                },

                "unity_delta" => new UnityDeltaBlock(),
                "unity_fixed_delta" => new UnityFixedDeltaBlock(),
                "unity_time" => new UnityTimeBlock(),

                "unity_wait" => new UnityWaitBlock
                {
                    T = ParseValue(dict.GetValueOrDefault("T"))
                },

                "unity_vec3" => new UnityVec3Block
                {
                    X = ParseValue(dict.GetValueOrDefault("X")),
                    Y = ParseValue(dict.GetValueOrDefault("Y")),
                    Z = ParseValue(dict.GetValueOrDefault("Z"))
                },

                "unity_vec3_zero" => new UnityVec3ZeroBlock(),
                "unity_vec3_up" => new UnityVec3UpBlock(),

                "unity_distance" => new UnityDistanceBlock
                {
                    A = ParseValue(dict.GetValueOrDefault("A")),
                    B = ParseValue(dict.GetValueOrDefault("B"))
                },

                "unity_lerp" => new UnityLerpBlock
                {
                    A = ParseValue(dict.GetValueOrDefault("A")),
                    B = ParseValue(dict.GetValueOrDefault("B")),
                    T = ParseValue(dict.GetValueOrDefault("T"))
                },

                "unity_normalize" => new UnityNormalizeBlock
                {
                    V = ParseValue(dict.GetValueOrDefault("V"))
                },

                _ => new UnknownBlock { Raw = dict }
            };
        }

        private static BlockBase? ParseBlockBase(Dictionary<string, object> dict)
        {
            object? parsed = ParseBlock(dict);

            if (parsed is not BlockBase block)
                return null;

            return block;
        }

        private static Dictionary<string, object?> ParseCallArgs(Dictionary<string, object> dict)
        {
            Dictionary<string, object?> args = [];
            int i = 0;

            while (dict.TryGetValue($"ARGNAME{i}", out object? argName) && dict.TryGetValue($"ARG{i}", out object? argVal))
            {
                args[argName.ToString()!] = argVal is Dictionary<string, object> d ? ParseBlock(d) : argVal;
                i++;
            }

            return args;
        }

        private static Vector3 ParseVector3(object? obj)
        {
            if (obj is Dictionary<string, object> dict)
            {
                return new Vector3(
                    ParseFloat(dict, "X"),
                    ParseFloat(dict, "Y"),
                    ParseFloat(dict, "Z")
                );
            }

            if (obj is Vector3 v)
                return v;

            throw new Exception($"Cannot convert {obj?.GetType().Name} to Vector3");
        }

        private static List<string> ParseParams(object obj)
        {
            List<string> list = [];

            if (obj is List<object> paramList)
            {
                foreach (object item in paramList)
                    list.Add(item.ToString());
            }

            return list;
        }

        private static object? ParseValue(object? value)
        {
            if (value is Dictionary<string, object> dict)
                return ParseBlock(dict);

            return value;
        }

        private static float ParseFloat(Dictionary<string, object> dict, string key, float defaultVal = 0f)
        {
            if (dict.TryGetValue(key, out var val) && float.TryParse(val?.ToString(), out float result))
                return result;

            return defaultVal;
        }

        private static bool ParseBool(Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var val) && bool.TryParse(val?.ToString(), out bool result))
                return result;

            return false;
        }

        private static string GetString(Dictionary<string, object> dict, string key)
            => dict.TryGetValue(key, out var val) ? val?.ToString() ?? string.Empty : string.Empty;

        private static List<Dictionary<string, object>> ParseStatementList(object stmtObj)
        {
            List<Dictionary<string, object>> list = [];

            if (stmtObj is List<object> objList)
            {
                foreach (object item in objList)
                {
                    if (item is Dictionary<object, object> dictObj)
                    {
                        Dictionary<string, object> newDict = [];
                        foreach (KeyValuePair<object, object> kvp in dictObj)
                        {
                            newDict[kvp.Key.ToString()] = kvp.Value;
                        }

                        list.Add(newDict);
                    }
                }
            }
            return list;
        }

        private static LogLevel GetLogLevel(string level)
        {
            return level switch
            {
                "info" => LogLevel.Info,
                "debug" => LogLevel.Debug,
                "warn" => LogLevel.Warn,
                "error" => LogLevel.Error,
                _ => LogLevel.Info
            };
        }
    }
}