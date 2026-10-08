using System;
using System.Linq;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders discrete categories as vertical bars.
    /// </summary>
    /// <param name="X">The center positions of the bars on the X axis.</param>
    /// <param name="Heights">The heights of the bars in data units.</param>
    /// <param name="BarWidth">The width of each bar in data units.</param>
    /// <param name="Label">A descriptive name for this series.</param>
    /// <param name="Color">The color of the bars.</param>
    public record BarChart(float[] X, float[] Heights, float BarWidth, string Label, Color Color) : Plot(Label, Color)
    {
        public override (Range xRange, Range yRange) GetValueRange()
        {
            if (X.Length == 0) return (new Range(0, 0), new Range(0, 0));

            var xMin = X.Min() - BarWidth / 2;
            var xMax = X.Max() + BarWidth / 2;
            var yMin = Math.Min(0, Heights.Min());
            var yMax = Math.Max(0, Heights.Max());

            return (new Range(xMin, xMax), new Range(yMin, yMax));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            for (var i = 0; i < X.Length; i++)
            {
                var (left, bottom) = map(X[i] - BarWidth / 2, 0);
                var (right, top) = map(X[i] + BarWidth / 2, Heights[i]);

                svg.Rect(left, top, right - left, bottom - top, Color);
            }
        }

        /// <summary>
        /// Calculates a "nice" default bar width based on the average spacing between bars.
        /// </summary>
        public static float CalculateDefaultWidth(float[] x)
        {
            if (x == null || x.Length < 2) return 0.8f;

            var spacings = Enumerable.Range(1, x.Length - 1)
                .Select(i => x[i] - x[i - 1]);
            return (float)spacings.Average() * 0.8f;
        }
    }
}