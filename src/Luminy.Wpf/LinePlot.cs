using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders a continuous polyline series using hardware-accelerated StreamGeometry.
    /// </summary>
    public class LinePlot : PlotElement
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(
                nameof(X),
                typeof(float[]),
                typeof(LinePlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(
                nameof(Y),
                typeof(float[]),
                typeof(LinePlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(LinePlot),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

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

        /// <summary> Gets or sets the line stroke thickness in DIP pixels. </summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (X == null || Y == null || X.Length == 0 || Y.Length == 0)
            {
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));
            }

            var validX = X.Where(float.IsFinite).ToArray();
            var validY = Y.Where(float.IsFinite).ToArray();

            if (validX.Length == 0 || validY.Length == 0)
            {
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));
            }

            return (
                new Luminy.Model.Range(validX.Min(), validX.Max()),
                new Luminy.Model.Range(validY.Min(), validY.Max())
            );
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (X == null || Y == null || X.Length < 2) return;

            var count = Math.Min(X.Length, Y.Length);
            var effectiveColor = Color ?? defaultColor;
            var brush = new SolidColorBrush(effectiveColor);
            brush.Freeze();

            var pen = new Pen(brush, StrokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };
            pen.Freeze();

            var segments = new List<List<Point>>();
            var currentSegment = new List<Point>();

            for (var i = 0; i < count; i++)
            {
                if (!float.IsFinite(X[i]) || !float.IsFinite(Y[i]))
                {
                    if (currentSegment.Count > 0)
                    {
                        segments.Add(currentSegment);
                        currentSegment = new List<Point>();
                    }
                    continue;
                }

                var (px, py) = map(X[i], Y[i]);
                currentSegment.Add(new Point(px, py));
            }

            if (currentSegment.Count > 0)
            {
                segments.Add(currentSegment);
            }

            foreach (var segment in segments)
            {
                if (segment.Count < 2) continue;

                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(segment[0], isFilled: false, isClosed: false);
                    ctx.PolyLineTo(segment.Skip(1).ToList(), isStroked: true, isSmoothJoin: true);
                }

                geometry.Freeze();
                dc.DrawGeometry(null, pen, geometry);
            }
        }
    }
}
