using System;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders a 2D array of data as a color-coded matrix (heatmap).
    /// </summary>
    /// <param name="Data">The 2D numerical array to visualize.</param>
    /// <param name="ColorMin">The color corresponding to the minimum value in the matrix.</param>
    /// <param name="ColorMax">The color corresponding to the maximum value in the matrix.</param>
    /// <param name="Label">A descriptive name for this matrix.</param>
    public record MatrixPlot(float[,] Data, Color ColorMin, Color ColorMax, string Label) : Plot(Label, Color.Black)
    {

        public override (Range xRange, Range yRange) GetValueRange()
        {
            GetDimensions(out var rows, out var cols);
            return (new Range(0.5f, cols + 0.5f), new Range(0.5f, rows + 0.5f));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            GetRange(out var rows, out var cols, out var min, out var max);

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    if (!float.IsFinite(Data[i, j])) continue;

                    var (left, top) = map(j+ 0.5f, i + 0.5f);
                    var (right, bottom) = map(j + 1.5f, i + 1.5f);

                    var width = right - left;
                    var height = top - bottom;

                    // Normalize value to [0, 1]
                    var normalized = (Data[i, j] - min) / (float.Abs(max - min) + 1e-15f);

                    // Interpolate color
                    var color = Color.Lerp(ColorMin, ColorMax, normalized);

                    svg.Rect(left, bottom, width, height, fill:color);
                }
            }
        }

        private void GetRange(out int rows, out int cols, out float min, out float max)
        {
            GetDimensions(out rows, out cols);

            min = float.MaxValue;
            max = float.MinValue;

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    if (!float.IsFinite(Data[i, j])) continue;
                    min = Math.Min(min, Data[i, j]);
                    max = Math.Max(max, Data[i, j]);
                }
            }
        }

        private void GetDimensions(out int rows, out int cols)
        {
            rows = Data.GetLength(0);
            cols = Data.GetLength(1);
        }
    }
}