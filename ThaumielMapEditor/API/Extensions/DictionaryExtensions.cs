using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThaumielMapEditor.API.Extensions
{
    internal static class DictionaryExtensions
    {
        public static bool TryConvertValue<TKey, TValue, T>(this Dictionary<TKey, TValue> dict, TKey key, out T result)
        {
            result = default!;

            if (!dict.TryGetValue(key, out TValue value) || value is null)
                return false;

            try
            {
                if (value is T direct)
                {
                    result = direct;
                    return true;
                }

                if (typeof(T) == typeof(Color))
                {
                    if (TryConvertToColor(value, out Color color))
                    {
                        result = (T)(object)color;
                        return true;
                    }

                    return false;
                }

                if (typeof(T).IsEnum)
                {
                    try
                    {
                        string enumStr = value.ToString()!.Replace(" ", "");
                        result = (T)Enum.Parse(typeof(T), enumStr, ignoreCase: true);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = typeof(T).GetGenericArguments()[0];

                    if (value is IEnumerable enumerable)
                    {
                        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                        foreach (object? item in enumerable)
                        {
                            try
                            {
                                list.Add(ConvertFromDictionary(item, elementType));
                            }
                            catch
                            {
                                return false;
                            }
                        }

                        result = (T)list;
                        return true;
                    }

                    return false;
                }

                if (typeof(T) == typeof(Vector2) || typeof(T) == typeof(Vector3) || typeof(T) == typeof(Vector4))
                {
                    try
                    {
                        if (value is Vector2 v2)
                        {
                            result = (T)(object)v2;
                            return true;
                        }

                        if (value is Vector3 v3)
                        {
                            result = (T)(object)v3;
                            return true;
                        }

                        if (value is Vector4 v4)
                        {
                            result = (T)(object)v4;
                            return true;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }

                result = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryConvertToColor(object value, out Color color)
        {
            color = default;

            if (value is string str)
            {
                str = str.Trim();

                if (str.StartsWith("#"))
                    return ColorUtility.TryParseHtmlString(str, out color);

                string[] parts = str.Split(',');
                if (parts.Length is 3 or 4)
                {
                    if (float.TryParse(parts[0].Trim(), out float r) && float.TryParse(parts[1].Trim(), out float g) && float.TryParse(parts[2].Trim(), out float b))
                    {
                        float a = parts.Length == 4 && float.TryParse(parts[3].Trim(), out float pa) ? pa : 1f;
                        color = new Color(r, g, b, a);
                        return true;
                    }
                }

                return false;
            }

            Dictionary<string, object>? colorDict = value switch
            {
                Dictionary<string, object> typed => typed,
                Dictionary<object, object> untyped => untyped.Where(kvp => kvp.Key != null).ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value),
                _ => null
            };

            if (colorDict is null)
                return false;

            colorDict.TryConvertValue("r", out float dr);
            colorDict.TryConvertValue("g", out float dg);
            colorDict.TryConvertValue("b", out float db);
            float da = colorDict.TryConvertValue("a", out float da2) ? da2 : 1f;

            color = new Color(dr, dg, db, da);
            return true;
        }

        public static bool TryConvertValue<T>(this Dictionary<string, object> dict, string key, out T result) =>
            dict.TryConvertValue<string, object, T>(key, out result);

        public static T GetConvertValue<T>(this Dictionary<string, object> dict, string key)
        {
            dict.TryConvertValue<string, object, T>(key, out T result);
            return result;
        }

        public static T GetConvertedValueOrDefault<TKey, TValue, T>(this Dictionary<TKey, TValue> dict, TKey key, T defaultValue = default!) =>
            dict.TryConvertValue(key, out T result) ? result : defaultValue;

        private static object? ConvertFromDictionary(object? item, Type targetType)
        {
            if (item is null)
                return null;

            if (targetType.IsInstanceOfType(item))
                return item;

            if (targetType.IsEnum)
            {
                string enumStr = item.ToString()!.Replace(" ", "");
                return Enum.Parse(targetType, enumStr, ignoreCase: true);
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                if (item is IEnumerable enumerable)
                {
                    IList list = (IList)Activator.CreateInstance(targetType)!;
                    foreach (object? element in enumerable)
                    {
                        list.Add(ConvertFromDictionary(element, elementType));
                    }

                    return list;
                }
                return null;
            }

            if (item is Dictionary<object, object> untyped)
                item = untyped.Where(k => k.Key != null).ToDictionary(k => k.Key.ToString()!, k => k.Value);

            if (item is Dictionary<string, object> dictItem)
            {
                object obj = Activator.CreateInstance(targetType)!;
                foreach (var prop in targetType.GetProperties())
                {
                    if (!dictItem.TryGetValue(prop.Name, out var propValue))
                        continue;

                    try
                    {
                        prop.SetValue(obj, ConvertFromDictionary(propValue, prop.PropertyType));
                    }
                    catch
                    {

                    }
                }

                return obj;
            }

            return Convert.ChangeType(item, targetType);
        }
    }
}