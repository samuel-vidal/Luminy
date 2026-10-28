using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders a frequency distribution of numerical data using hardware-accelerated DirectX DrawingContext.
    /// </summary>
    public class HistogramPlot : PlotElement
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(float[]), typeof(HistogramPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BinCountProperty =
            DependencyProperty.Register(nameof(BinCount), typeof(int), typeof(HistogramPlot),
                new FrameworkPropertyMetadata(20, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the numerical sample data to bin and visualize. </summary>
        public float[]? Data
        {
            get => (float[]?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary> Gets or sets the number of bins for the frequency distribution. </summary>
        public int BinCount
        {
            get => (int)GetValue(BinCountProperty);
            set => SetValue(BinCountProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (Data == null || Data.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var validData = Data.Where(float.IsFinite).ToArray();
            if (validData.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var min = validData.Min();
            var max = validData.Max();
            var bins = ComputeBins(validData, min, max, Math.Max(1, BinCount));

            return (new Luminy.Model.Range(min, max), new Luminy.Model.Range(0, bins.Max()));
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (Data == null || Data.Length == 0) return;

            var validData = Data.Where(float.IsFinite).ToArray();
            if (validData.Length == 0) return;

            var effectiveColor = Color ?? defaultColor;
            var brush = new SolidColorBrush(effectiveColor);
            brush.Freeze();

            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255)), 0.5);
            pen.Freeze();

            var min = validData.Min();
            var max = validData.Max();
            var numBins = Math.Max(1, BinCount);

            var span = max - min;
            if (span < 1e-10f)
            {
                min -= 0.5f;
                max += 0.5f;
                span = 1.0f;
            }

            var binWidth = span / numBins;
            var bins = ComputeBins(validData, min, max, numBins);

            for (var i = 0; i < numBins; i++)
            {
                var xLeft = min + i * binWidth;
                var xRight = xLeft + binWidth;
                var height = bins[i];

                var (screenLeft, screenBottom) = map(xLeft, 0);
                var (screenRight, screenTop) = map(xRight, height);

                var rx = Math.Min(screenLeft, screenRight);
                var ry = Math.Min(screenTop, screenBottom);
                var rw = Math.Abs(screenRight - screenLeft);
                var rh = Math.Abs(screenBottom - screenTop);

                if (rw > 0 && rh > 0)
                {
                    dc.DrawRectangle(brush, pen, new Rect(rx, ry, rw, rh));
                }
            }
        }

        private static int[] ComputeBins(float[] data, float min, float max, int binCount)
        {
            var span = max - min;
            if (span < 1e-10f) span = 1.0f;

            var binWidth = span / binCount;
            var bins = new int[binCount];

            foreach (var value in data)
            {
                var binIndex = Math.Min(binCount - 1, (int)((value - min) / binWidth));
                if (binIndex < 0) binIndex = 0;
                bins[binIndex]++;
            }

            return bins;
        }
    }
}
