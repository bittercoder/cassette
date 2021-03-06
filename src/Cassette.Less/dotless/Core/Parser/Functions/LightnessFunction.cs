﻿namespace dotless.Core.Parser.Functions
{
    using Infrastructure.Nodes;
    using Tree;
    using Utils;

    class LightnessFunction : HslColorFunctionBase
    {
        protected override Node EvalHsl(HslColor color)
        {
            return color.GetLightness();
        }

        protected override Node EditHsl(HslColor color, Number number)
        {
            color.Lightness += number.Value/100;
            return color.ToRgbColor();
        }
    }

    class LightenFunction : LightnessFunction {}

    class DarkenFunction : LightnessFunction
    {
        protected override Node EditHsl(HslColor color, Number number)
        {
            return base.EditHsl(color, -number);
        }
    }
}