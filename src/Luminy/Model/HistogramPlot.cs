using System;
using System.Linq;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Renders a frequency distribution of numerical data.
    /// </summary>
    /// <param name="Data">The numerical data to bin.</param>
    /// <param name="BinCount">The number of bins to use for the distribution.</param>
    /// <param name="Label">A descriptive name for this series.</param>
    /// <param name="Color">The color of the histogram bars.</param>
    public record HistogramPlot(float[] Data, int BinCount, string Label, Color Color) : Plot(Label, Color)
    {
        public override (Range xRange, Range yRange) GetValueRange()
        {
            if (Data.Length == 0) return (new Range(0, 0), new Range(0, 0));

            var min = Data.Min();
            var max = Data.Max();

            // Create histogram bins
            var span = max - min;
            if (span < 1e-10) span = 1.0f;
            var binWidth = span / BinCount;
            var bins = new int[BinCount];

            foreach (var value in Data)
            {
                var binIndex = Math.Min(BinCount - 1, (int)((value - min) / binWidth));
                if (binIndex < 0) binIndex = 0;
                bins[binIndex]++;
            }

            return (new Range(min, max), new Range(0, bins.Max()));
        }

        public override void Render(SvgBuilder svg, CoordinateMap map)
        {
            if (Data.Length == 0) return;

            var min = Data.Min();
            var max = Data.Max();

            var span = max - min;
            if (span < 1e-10)
            {
                min -= 0.5f;
                max += 0.5f;
                span = 1.0f;
            }

            var binWidth = span / BinCount;
            var bins = new int[BinCount];

            foreach (var value in Data)
            {
                var binIndex = Math.Min(BinCount - 1, (int)((value - min) / binWidth));
                if (binIndex < 0) binIndex = 0;
                bins[binIndex]++;
            }

            for (var i = 0; i < BinCount; i++)
            {
                var xLeft = min + i * binWidth;
                var xRight = xLeft + binWidth;
                var height = bins[i];

                var (screenLeft, screenBottom) = map(xLeft, 0);
                var (screenRight, screenTop) = map(xRight, height);

                var rectHeight = screenBottom - screenTop;
                svg.Rect(screenLeft, screenTop, screenRight - screenLeft, rectHeight, fill: Color);
            }
        }
    }
}