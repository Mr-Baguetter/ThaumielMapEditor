// -----------------------------------------------------------------------
// <copyright file="MathBlocks.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using UnityEngine;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class MathArithmeticBlock : BlockBase
    {
        public string OP { get; set; } = "ADD";
        public object? A { get; set; }
        public object? B { get; set; }

        public override object ReturnExecute()
        {
            float a = ResolveFloat(A);
            float b = ResolveFloat(B);

            float result = OP switch
            {
                "ADD" => a + b,
                "MINUS" => a - b,
                "MULTIPLY" => a * b,
                "DIVIDE" => b != 0f ? a / b : 0f,
                "POWER" => Mathf.Pow(a, b),
                _ => 0f
            };

            LogManager.Debug($"{a} {OP} {b} = {result}");
            return result;
        }
    }

    public class MathSingleBlock : BlockBase
    {
        public string OP { get; set; } = "ABS";
        public object? NUM { get; set; }

        public override object ReturnExecute()
        {
            float num = ResolveFloat(NUM);

            float result = OP switch
            {
                "ROOT" => Mathf.Sqrt(num),
                "ABS" => Mathf.Abs(num),
                "NEG" => -num,
                "LN" => Mathf.Log(num),
                "LOG10" => Mathf.Log10(num),
                "EXP" => Mathf.Exp(num),
                "POW10" => Mathf.Pow(10f, num),
                _ => 0f
            };

            LogManager.Debug($"{OP}({num}) = {result}");
            return result;
        }
    }

    public class MathTrigBlock : BlockBase
    {
        public string OP { get; set; } = "SIN";
        public object? NUM { get; set; }

        public override object ReturnExecute()
        {
            float degrees = ResolveFloat(NUM);
            float radians = degrees * Mathf.Deg2Rad;

            float result = OP switch
            {
                "SIN" => Mathf.Sin(radians),
                "COS" => Mathf.Cos(radians),
                "TAN" => Mathf.Tan(radians),
                "ASIN" => Mathf.Asin(degrees) * Mathf.Rad2Deg,
                "ACOS" => Mathf.Acos(degrees) * Mathf.Rad2Deg,
                "ATAN" => Mathf.Atan(degrees) * Mathf.Rad2Deg,
                _ => 0f
            };

            LogManager.Debug($"{OP}({degrees}°) = {result}");
            return result;
        }
    }

    public class MathRoundBlock : BlockBase
    {
        public string OP { get; set; } = "ROUND";
        public object? NUM { get; set; }

        public override object ReturnExecute()
        {
            float num = ResolveFloat(NUM);

            float result = OP switch
            {
                "ROUND" => Mathf.Round(num),
                "ROUNDUP" => Mathf.Ceil(num),
                "ROUNDDOWN" => Mathf.Floor(num),
                _ => num
            };

            LogManager.Debug($"{OP}({num}) = {result}");
            return result;
        }
    }

    public class MathModuloBlock : BlockBase
    {
        public object? DIVIDEND { get; set; }
        public object? DIVISOR { get; set; }

        public override object ReturnExecute()
        {
            float dividend = ResolveFloat(DIVIDEND);
            float divisor = ResolveFloat(DIVISOR, 1f);

            float result = divisor != 0f ? dividend % divisor : 0f;

            LogManager.Debug($"{dividend} % {divisor} = {result}");
            return result;
        }
    }

    public class MathConstrainBlock : BlockBase
    {
        public object? VALUE { get; set; }
        public object? LOW { get; set; }
        public object? HIGH { get; set; }

        public override object ReturnExecute()
        {
            float value = ResolveFloat(VALUE);
            float low = ResolveFloat(LOW, 0f);
            float high = ResolveFloat(HIGH, 1f);

            float result = Mathf.Clamp(value, low, high);

            LogManager.Debug($"Clamp({value}, {low}, {high}) = {result}");
            return result;
        }
    }

    public class MathRandomFloatBlock : BlockBase
    {
        public override object ReturnExecute()
        {
            float result = Random.value;
            LogManager.Debug($"{result}");
            return result;
        }
    }
}