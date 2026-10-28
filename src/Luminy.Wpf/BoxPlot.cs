using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders a statistical summary of data using Tukey-style box, median line, whiskers, and outlier points.
    /// </summary>
    public class BoxPlot : PlotElement
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(float[]), typeof(BoxPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(float), typeof(BoxPlot),
                new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BoxWidthProperty =
            DependencyProperty.Register(nameof(BoxWidth), typeof(float), typeof(BoxPlot),
                new FrameworkPropertyMetadata(0.8f, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the sample data points to summarize statistically. </summary>
        public float[]? Data
        {
            get => (float[]?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary> Gets or sets the center position of the box on the X axis. </summary>
        public float X
        {
            get => (float)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary> Gets or sets the width of the box in data units. </summary>
        public float BoxWidth
        {
            get => (float)GetValue(BoxWidthProperty);
            set => SetValue(BoxWidthProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (Data == null || Data.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var validData = Data.Where(float.IsFinite).OrderBy(v => v).ToArray();
            if (validData.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            return (
                new Luminy.Model.Range(X - BoxWidth / 2f, X + BoxWidth / 2f),
                new Luminy.Model.Range(validData[0], validData[^1])
            );
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (Data == null || Data.Length == 0) return;

            var sortedData = Data.Where(float.IsFinite).OrderBy(v => v).ToArray();
            if (sortedData.Length == 0) return;

            var effectiveColor = Color ?? defaultColor;
            var strokeBrush = new SolidColorBrush(effectiveColor);
            strokeBrush.Freeze();

            var fillBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, effectiveColor.R, effectiveColor.G, effectiveColor.B));
            fillBrush.Freeze();

            var mainPen = new Pen(strokeBrush, 1.5);
            mainPen.Freeze();

            var medianPen = new Pen(strokeBrush, 2.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            medianPen.Freeze();

            var q1 = sortedData[sortedData.Length / 4];
            var median = sortedData[sortedData.Length / 2];
            var q3 = sortedData[sortedData.Length * 3 / 4];

            var iqr = q3 - q1;
            var lowerWhisker = sortedData.First(v => v >= q1 - 1.5f * iqr);
            var upperWhisker = sortedData.Last(v => v <= q3 + 1.5f * iqr);

            // Coordinates
            var (left, q1y) = map(X - BoxWidth / 2f, q1);
            var (right, q3y) = map(X + BoxWidth / 2f, q3);
            var (_, medianY) = map(X, median);
            var (_, lowerWhiskerY) = map(X, lowerWhisker);
            var (_, upperWhiskerY) = map(X, upperWhisker);
            var (boxCenterX, _) = map(X, 0);

            // Box
            var boxRect = new Rect(
                Math.Min(left, right),
                Math.Min(q1y, q3y),
                Math.Abs(right - left),
                Math.Abs(q1y - q3y));
            dc.DrawRectangle(fillBrush, mainPen, boxRect);

            // Median line
            dc.DrawLine(medianPen, new Point(left, medianY), new Point(right, medianY));

            // Whiskers
            dc.DrawLine(mainPen, new Point(boxCenterX, Math.Max(q1y, q3y)), new Point(boxCenterX, lowerWhiskerY));
            dc.DrawLine(mainPen, new Point(boxCenterX, Math.Min(q1y, q3y)), new Point(boxCenterX, upperWhiskerY));

            // Whisker caps
            var (capLeft, _) = map(X - BoxWidth / 4f, 0);
            var (capRight, _) = map(X + BoxWidth / 4f, 0);
            dc.DrawLine(mainPen, new Point(capLeft, lowerWhiskerY), new Point(capRight, lowerWhiskerY));
            dc.DrawLine(mainPen, new Point(capLeft, upperWhiskerY), new Point(capRight, upperWhiskerY));

            // Outliers
            var outliers = sortedData.Where(v => v < lowerWhisker || v > upperWhisker);
            foreach (var outlier in outliers)
            {
                var (_, outlierY) = map(X, outlier);
                dc.DrawEllipse(strokeBrush, null, new Point(boxCenterX, outlierY), 2.5, 2.5);
            }
        }
    }
}
