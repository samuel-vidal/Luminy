using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders a series of points connected by straight line segments.
    /// </summary>
    /// <param name="X">The X coordinates of the data points.</param>
    /// <param name="Y">The Y coordinates of the data points.</param>
    /// <param name="Label">A descriptive name for this data series.</param>
    /// <param name="Color">The color of the line.</param>
    /// <param name="StrokeWidth">The width of the line stroke in pixels.</param>
    public record LinePlot(float[] X, float[] Y, string Label, Color Color, int StrokeWidth = 2) : Plot(Label, Color)
    {

        public override (Range xRange, Range yRange) GetValueRange()
        {
            if (X.Length == 0) return (new Range(0, 0), new Range(0, 0));

            var xMin = X.Where(float.IsFinite).Min();
            var xMax = X.Where(float.IsFinite).Max();
            var yMin = Y.Where(float.IsFinite).Min();
            var yMax = Y.Where(float.IsFinite).Max();

            return (new Range(xMin, xMax), new Range(yMin, yMax));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            if (X.Length < 2) return;

            var segments = new List<List<(float x, float y)>>();
            var currentSegment = new List<(float, float)>();

            for (var i = 0; i < X.Length; i++)
            {
                // Skip invalid points
                if (!IsValidPoint(X[i], Y[i]))
                {
                    // End current segment if we have points
                    if (currentSegment.Count > 0)
                    {
                        segments.Add(currentSegment);
                        currentSegment = new List<(float, float)>();
                    }
                    continue;
                }

                var (px, py) = map(X[i], Y[i]);
                currentSegment.Add((px, py));
            }

            // Add the last segment if it has points
            if (currentSegment.Count > 0)
                segments.Add(currentSegment);

            // Render each segment as a separate path
            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;

                var pathBuilder = new StringBuilder();
                pathBuilder.Append($"M {segment[0].x} {segment[0].y}");

                for (var i = 1; i < segment.Count; i++)
                {
                    pathBuilder.Append($" L {segment[i].x} {segment[i].y}");
                }

                svg.Path(pathBuilder.ToString(), stroke: Color, strokeWidth: StrokeWidth);
            }
        }
    }
}