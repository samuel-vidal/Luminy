using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders discrete categories as vertical bars using hardware-accelerated DirectX DrawingContext.
    /// </summary>
    public class BarChart : PlotElement
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(float[]), typeof(BarChart),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HeightsProperty =
            DependencyProperty.Register(nameof(Heights), typeof(float[]), typeof(BarChart),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register(nameof(BarWidth), typeof(float?), typeof(BarChart),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the center positions of the bars on the X axis. </summary>
        public float[]? X
        {
            get => (float[]?)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary> Gets or sets the heights of the bars in data units. </summary>
        public float[]? Heights
        {
            get => (float[]?)GetValue(HeightsProperty);
            set => SetValue(HeightsProperty, value);
        }

        /// <summary> Gets or sets the width of each bar in data units. If null, calculates an optimal width automatically. </summary>
        public float? BarWidth
        {
            get => (float?)GetValue(BarWidthProperty);
            set => SetValue(BarWidthProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (X == null || Heights == null || X.Length == 0 || Heights.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var validX = X.Where(float.IsFinite).ToArray();
            var validH = Heights.Where(float.IsFinite).ToArray();

            if (validX.Length == 0 || validH.Length == 0)
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            var width = GetEffectiveBarWidth();
            var xMin = validX.Min() - width / 2f;
            var xMax = validX.Max() + width / 2f;
            var yMin = Math.Min(0f, validH.Min());
            var yMax = Math.Max(0f, validH.Max());

            return (new Luminy.Model.Range(xMin, xMax), new Luminy.Model.Range(yMin, yMax));
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (X == null || Heights == null || X.Length == 0 || Heights.Length == 0) return;

            var effectiveColor = Color ?? defaultColor;
            var brush = new SolidColorBrush(effectiveColor);
            brush.Freeze();

            var width = GetEffectiveBarWidth();
            var halfWidth = width / 2f;
            var count = Math.Min(X.Length, Heights.Length);

            for (var i = 0; i < count; i++)
            {
                var xi = X[i];
                var hi = Heights[i];
                if (!float.IsFinite(xi) || !float.IsFinite(hi)) continue;

                var (left, bottom) = map(xi - halfWidth, 0);
                var (right, top) = map(xi + halfWidth, hi);

                var rx = Math.Min(left, right);
                var ry = Math.Min(top, bottom);
                var rw = Math.Abs(right - left);
                var rh = Math.Abs(bottom - top);

                if (rw > 0 && rh > 0)
                {
                    dc.DrawRectangle(brush, null, new Rect(rx, ry, rw, rh));
                }
            }
        }

        private float GetEffectiveBarWidth()
        {
            if (BarWidth.HasValue && BarWidth.Value > 0)
                return BarWidth.Value;

            return X != null ? Luminy.Model.BarChart.CalculateDefaultWidth(X) : 0.8f;
        }
    }
}
