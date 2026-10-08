using System;
using System.Linq;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders individual data points as markers (circles).
    /// </summary>
    /// <param name="X">The X coordinates of the points.</param>
    /// <param name="Y">The Y coordinates of the points.</param>
    /// <param name="Label">A descriptive name for this series.</param>
    /// <param name="Color">The color of the markers.</param>
    /// <param name="PointSize">The radius of each point in pixels.</param>
    public record PointPlot(float[] X, float[] Y, string Label, Color Color, int PointSize = 3) : Plot(Label, Color)
    {
        public override (Range xRange, Range yRange) GetValueRange()
        {
            if (X.Length == 0) return (new Range(0, 0), new Range(0, 0));

            var xMin = X.Min();
            var xMax = X.Max();
            var yMin = Y.Min();
            var yMax = Y.Max();

            return (new Range(xMin, xMax), new Range(yMin, yMax));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            foreach (var (x, y) in X.Zip(Y, (x, y) => (x, y)))
            {
                var (px, py) = map(x, y);
                svg.Circle(px, py, PointSize, Color);
            }
        }

        /// <summary>
        /// Filters out non-finite points (NaN, Infinity) from the data.
        /// </summary>
        public static (float[] cleanX, float[] cleanY) CleanData(float[] x, float[] y)
        {
            if (x == null || y == null) return (Array.Empty<float>(), Array.Empty<float>());
            
            var validPairs = x.Zip(y, (xVal, yVal) => (xVal, yVal))
                .Where(pair => float.IsFinite(pair.xVal) && float.IsFinite(pair.yVal))
                .ToArray();

            return (validPairs.Select(p => p.xVal).ToArray(), 
                validPairs.Select(p => p.yVal).ToArray());
        }
    }
}