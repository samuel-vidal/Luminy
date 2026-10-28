using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders individual data points as circular scatter markers using DirectX DrawingContext.
    /// </summary>
    public class PointPlot : PlotElement
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(float[]), typeof(PointPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(nameof(Y), typeof(float[]), typeof(PointPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PointSizeProperty =
            DependencyProperty.Register(nameof(PointSize), typeof(double), typeof(PointPlot),
                new FrameworkPropertyMetadata(3.5, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the X coordinates of the data points. </summary>
        public float[]? X
        {
            get => (float[]?)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary> Gets or sets the Y coordinates of the data points. </summary>
        public float[]? Y
        {
            get => (float[]?)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        /// <summary> Gets or sets the radius of each marker in DIP pixels. </summary>
        public double PointSize
        {
            get => (double)GetValue(PointSizeProperty);
            set => SetValue(PointSizeProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (X == null || Y == null || X.Length == 0 || Y.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var validX = X.Where(float.IsFinite).ToArray();
            var validY = Y.Where(float.IsFinite).ToArray();

            if (validX.Length == 0 || validY.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            return (
                new Luminy.Model.Range(validX.Min(), validX.Max()),
                new Luminy.Model.Range(validY.Min(), validY.Max())
            );
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (X == null || Y == null || X.Length == 0 || Y.Length == 0) return;

            var effectiveColor = Color ?? defaultColor;
            var brush = new SolidColorBrush(effectiveColor);
            brush.Freeze();

            var count = Math.Min(X.Length, Y.Length);
            var radius = Math.Max(0.5, PointSize);

            for (var i = 0; i < count; i++)
            {
                var xi = X[i];
                var yi = Y[i];
                if (!float.IsFinite(xi) || !float.IsFinite(yi)) continue;

                var (px, py) = map(xi, yi);
                dc.DrawEllipse(brush, null, new Point(px, py), radius, radius);
            }
        }
    }
}
