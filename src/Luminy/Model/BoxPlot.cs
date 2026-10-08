using System.Linq;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders a statistical summary of data using whiskers and a box (Tukey style).
    /// </summary>
    /// <param name="Data">The raw data points to summarize.</param>
    /// <param name="X">The center position of the box on the X axis.</param>
    /// <param name="Label">A descriptive name for this series.</param>
    /// <param name="Color">The color of the box and whiskers.</param>
    /// <param name="Width">The width of the box in data units.</param>
    public record BoxPlot(float[] Data, float X, string Label, Color Color, float Width = 0.8f) : Plot(Label, Color)
    {
        public override (Range xRange, Range yRange) GetValueRange()
        {
            if (Data.Length == 0) return (new Range(0, 0), new Range(0, 0));

            var sortedData = Data.OrderBy(x => x).ToArray();
            var min = sortedData[0];
            var max = sortedData[^1];

            return (new Range(X - Width / 2, X + Width / 2), new Range(min, max));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            if (Data.Length == 0) return;

            var sortedData = Data.OrderBy(x => x).ToArray();
            var min = sortedData[0];
            var max = sortedData[^1];
            var q1 = sortedData[sortedData.Length / 4];
            var median = sortedData[sortedData.Length / 2];
            var q3 = sortedData[sortedData.Length * 3 / 4];

            // Calculate IQR and whiskers
            var iqr = q3 - q1;
            var lowerWhisker = sortedData.First(x => x >= q1 - 1.5f * iqr);
            var upperWhisker = sortedData.Last(x => x <= q3 + 1.5f * iqr);

            // Coordinates
            var (left, q1y) = map(X - Width / 2, q1);
            var (right, q3y) = map(X + Width / 2, q3);
            var (centerX, medianY) = map(X, median);
            var (_, lowerWhiskerY) = map(X, lowerWhisker);
            var (_, upperWhiskerY) = map(X, upperWhisker);
            var (boxCenterX, _) = map(X, 0);

            // Box
            svg.Rect(left, q3y, right - left, q1y - q3y, fill: null, stroke: Color);

            // Median line
            svg.Line(left, medianY, right, medianY, Color, 2);

            // Whiskers
            svg.Line(boxCenterX, q1y, boxCenterX, lowerWhiskerY, Color);
            svg.Line(boxCenterX, q3y, boxCenterX, upperWhiskerY, Color);

            // Whisker caps
            var (capLeft, _) = map(X - Width / 4, 0);
            var (capRight, _) = map(X + Width / 4, 0);
            svg.Line(capLeft, lowerWhiskerY, capRight, lowerWhiskerY, Color);
            svg.Line(capLeft, upperWhiskerY, capRight, upperWhiskerY, Color);

            // Outliers
            var outliers = sortedData.Where(x => x < lowerWhisker || x > upperWhisker);
            foreach (var outlier in outliers)
            {
                var (_, outlierY) = map(X, outlier);
                svg.Circle(boxCenterX, outlierY, 2, Color);
            }
        }
    }
}