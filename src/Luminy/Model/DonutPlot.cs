using System;
using System.Collections.Generic;
using System.Globalization;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Specifies how numerical values are displayed inside donut slices.
    /// </summary>
    public enum DonutValueDisplay
    {
        /// <summary> No numbers inside the slices. </summary>
        None,
        /// <summary> Display the raw numerical value. </summary>
        Raw,
        /// <summary> Display the percentage of the slice. </summary>
        Percent
    }

    /// <summary>
    /// Renders a circular donut or pie chart with inner and outer radii.
    /// </summary>
    /// <param name="Values">The numerical values for each slice.</param>
    /// <param name="Labels">The descriptive labels for each slice.</param>
    /// <param name="ExternalRadius">The outer radius of the donut in data units.</param>
    /// <param name="InternalRadius">The inner radius of the donut in data units (0 for a pie chart).</param>
    /// <param name="Colors">Optional colors for each slice. If null, default palette colors are used.</param>
    /// <param name="ShowSliceLabels">Whether to display horizontal labels outside each slice at 110% radius.</param>
    /// <param name="InsideValueDisplay">How to display numerical values inside the slices.</param>
    /// <param name="MinInsideValueThreshold">Minimum percentage (between 0 and 1) required to display an inside value.</param>
    /// <param name="Label">Optional descriptive name for this plot.</param>
    public record DonutPlot(
        float[] Values,
        string[] Labels,
        float ExternalRadius = 1.0f,
        float InternalRadius = 0.6f,
        Color[]? Colors = null,
        bool ShowSliceLabels = true,
        DonutValueDisplay InsideValueDisplay = DonutValueDisplay.None,
        float MinInsideValueThreshold = 0.05f,
        string Label = ""
    ) : Plot(Label, Color.Black)
    {
        public override (Range xRange, Range yRange) GetValueRange()
        {
            var margin = Math.Max(ExternalRadius * 1.35f, 0.1f);
            return (new Range(-margin, margin), new Range(-margin, margin));
        }

        public override IEnumerable<(string Label, Color Color)> GetLegendItems()
        {
            var colors = GetEffectiveColors();
            var count = Math.Min(Values.Length, Labels.Length);
            for (var i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(Labels[i]))
                {
                    yield return (Labels[i], colors[i]);
                }
            }
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            if (Values == null || Values.Length == 0) return;

            var validIndices = new List<int>();
            var total = 0f;
            for (var i = 0; i < Values.Length; i++)
            {
                if (float.IsFinite(Values[i]) && Values[i] > 0)
                {
                    validIndices.Add(i);
                    total += Values[i];
                }
            }

            if (total <= 0 || validIndices.Count == 0) return;

            var (cx, cy) = map(0, 0);
            var (rx, _) = map(ExternalRadius, 0);
            var (_, ry) = map(0, ExternalRadius);
            var rExtPixels = Math.Min(Math.Abs(rx - cx), Math.Abs(ry - cy));
            var rIntPixels = ExternalRadius > 0
                ? rExtPixels * (Math.Clamp(InternalRadius, 0f, ExternalRadius) / ExternalRadius)
                : 0f;

            var colors = GetEffectiveColors();
            var currentAngle = -MathF.PI / 2f;

            foreach (var index in validIndices)
            {
                var val = Values[index];
                var ratio = val / total;
                var sweepAngle = ratio * MathF.PI * 2f;
                var startAngle = currentAngle;
                var endAngle = currentAngle + sweepAngle;
                currentAngle = endAngle;

                var sliceColor = colors[index];

                if (ratio >= 0.9999f)
                {
                    RenderFullDonut(svg, cx, cy, rExtPixels, rIntPixels, sliceColor);
                }
                else
                {
                    RenderSlice(svg, cx, cy, rExtPixels, rIntPixels, startAngle, endAngle, sweepAngle, sliceColor);
                }

                var midAngle = startAngle + sweepAngle / 2f;

                if (InsideValueDisplay != DonutValueDisplay.None && ratio >= MinInsideValueThreshold)
                {
                    RenderInsideValue(svg, cx, cy, rExtPixels, rIntPixels, midAngle, val, ratio, sliceColor);
                }

                if (ShowSliceLabels && index < Labels.Length && !string.IsNullOrEmpty(Labels[index]))
                {
                    RenderOutsideLabel(svg, cx, cy, rExtPixels, midAngle, Labels[index]);
                }
            }
        }

        private static void RenderSlice(SvgBuilder svg, float cx, float cy, float rExt, float rInt, float startAngle, float endAngle, float sweepAngle, Color color)
        {
            var x1Ext = cx + rExt * MathF.Cos(startAngle);
            var y1Ext = cy + rExt * MathF.Sin(startAngle);
            var x2Ext = cx + rExt * MathF.Cos(endAngle);
            var y2Ext = cy + rExt * MathF.Sin(endAngle);

            var largeArc = sweepAngle > MathF.PI ? 1 : 0;
            string path;

            if (rInt <= 0.001f)
            {
                path = FormattableString.Invariant(
                    $"M {x1Ext:F3} {y1Ext:F3} A {rExt:F3} {rExt:F3} 0 {largeArc} 1 {x2Ext:F3} {y2Ext:F3} L {cx:F3} {cy:F3} Z");
            }
            else
            {
                var x1Int = cx + rInt * MathF.Cos(startAngle);
                var y1Int = cy + rInt * MathF.Sin(startAngle);
                var x2Int = cx + rInt * MathF.Cos(endAngle);
                var y2Int = cy + rInt * MathF.Sin(endAngle);

                path = FormattableString.Invariant(
                    $"M {x1Ext:F3} {y1Ext:F3} A {rExt:F3} {rExt:F3} 0 {largeArc} 1 {x2Ext:F3} {y2Ext:F3} L {x2Int:F3} {y2Int:F3} A {rInt:F3} {rInt:F3} 0 {largeArc} 0 {x1Int:F3} {y1Int:F3} Z");
            }

            svg.Path(path, fill: color, stroke: Color.White, strokeWidth: 1.5f);
        }

        private static void RenderFullDonut(SvgBuilder svg, float cx, float cy, float rExt, float rInt, Color color)
        {
            if (rInt <= 0.001f)
            {
                svg.Circle(cx, cy, rExt, fill: color, stroke: Color.White, strokeWidth: 1.5f);
            }
            else
            {
                var path = FormattableString.Invariant(
                    $"M {cx - rExt:F3} {cy:F3} A {rExt:F3} {rExt:F3} 0 1 0 {cx + rExt:F3} {cy:F3} A {rExt:F3} {rExt:F3} 0 1 0 {cx - rExt:F3} {cy:F3} M {cx - rInt:F3} {cy:F3} A {rInt:F3} {rInt:F3} 0 1 1 {cx + rInt:F3} {cy:F3} A {rInt:F3} {rInt:F3} 0 1 1 {cx - rInt:F3} {cy:F3} Z");
                svg.Path(path, fill: color, stroke: Color.White, strokeWidth: 1.5f);
            }
        }

        private void RenderInsideValue(SvgBuilder svg, float cx, float cy, float rExt, float rInt, float midAngle, float val, float ratio, Color sliceColor)
        {
            var rMid = (rInt + rExt) / 2f;
            var ix = cx + rMid * MathF.Cos(midAngle);
            var iy = cy + rMid * MathF.Sin(midAngle);

            var text = InsideValueDisplay == DonutValueDisplay.Percent
                ? FormattableString.Invariant($"{(ratio * 100f):0.#}%")
                : val.ToString("0.##", CultureInfo.InvariantCulture);

            var luminance = sliceColor.Red * 0.299f + sliceColor.Green * 0.587f + sliceColor.Blue * 0.114f;
            var textColor = luminance > 0.8f ? Color.Black : Color.White;

            svg.Text(text, ix, iy + 4, "Arial, sans-serif", 11, "middle", textColor, fontWeight: "bold");
        }

        private static void RenderOutsideLabel(SvgBuilder svg, float cx, float cy, float rExt, float midAngle, string label)
        {
            var cosMid = MathF.Cos(midAngle);
            var sinMid = MathF.Sin(midAngle);

            var rLabel = rExt * 1.10f;
            var lx = cx + rLabel * cosMid;
            var ly = cy + rLabel * sinMid;

            var textAnchor = cosMid switch
            {
                > 0.3f => "start",
                < -0.3f => "end",
                _ => "middle"
            };

            var yOffset = sinMid switch
            {
                < -0.3f => -2f,
                > 0.3f => 10f,
                _ => 4f
            };

            svg.Text(label, lx, ly + yOffset, "Arial, sans-serif", 11, textAnchor, Color.Black);
        }

        private Color[] GetEffectiveColors()
        {
            var result = new Color[Values.Length];
            for (var i = 0; i < Values.Length; i++)
            {
                result[i] = (Colors != null && i < Colors.Length)
                    ? Colors[i]
                    : LuminyBuilder.DefaultPalette[i % LuminyBuilder.DefaultPalette.Length];
            }
            return result;
        }
    }
}
