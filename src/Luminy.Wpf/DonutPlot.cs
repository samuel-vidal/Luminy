using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders a circular donut or pie chart using hardware-accelerated DirectX StreamGeometry.
    /// </summary>
    public class DonutPlot : PlotElement
    {
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(nameof(Values), typeof(float[]), typeof(DonutPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LabelsProperty =
            DependencyProperty.Register(nameof(Labels), typeof(string[]), typeof(DonutPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ExternalRadiusProperty =
            DependencyProperty.Register(nameof(ExternalRadius), typeof(float), typeof(DonutPlot),
                new FrameworkPropertyMetadata(1.0f, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty InternalRadiusProperty =
            DependencyProperty.Register(nameof(InternalRadius), typeof(float), typeof(DonutPlot),
                new FrameworkPropertyMetadata(0.6f, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty InsideValueDisplayProperty =
            DependencyProperty.Register(nameof(InsideValueDisplay), typeof(DonutValueDisplay), typeof(DonutPlot),
                new FrameworkPropertyMetadata(DonutValueDisplay.None, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the numerical values for each slice. </summary>
        public float[]? Values
        {
            get => (float[]?)GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        /// <summary> Gets or sets the descriptive labels for each slice. </summary>
        public string[]? Labels
        {
            get => (string[]?)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        /// <summary> Gets or sets the outer radius in data units. </summary>
        public float ExternalRadius
        {
            get => (float)GetValue(ExternalRadiusProperty);
            set => SetValue(ExternalRadiusProperty, value);
        }

        /// <summary> Gets or sets the inner radius in data units (0 for a pie chart). </summary>
        public float InternalRadius
        {
            get => (float)GetValue(InternalRadiusProperty);
            set => SetValue(InternalRadiusProperty, value);
        }

        /// <summary> Gets or sets how numerical values are displayed inside donut slices. </summary>
        public DonutValueDisplay InsideValueDisplay
        {
            get => (DonutValueDisplay)GetValue(InsideValueDisplayProperty);
            set => SetValue(InsideValueDisplayProperty, value);
        }

        private static readonly System.Windows.Media.Color[] SlicePalette =
        {
            System.Windows.Media.Color.FromRgb(31, 119, 180),
            System.Windows.Media.Color.FromRgb(255, 127, 14),
            System.Windows.Media.Color.FromRgb(44, 160, 44),
            System.Windows.Media.Color.FromRgb(214, 39, 40),
            System.Windows.Media.Color.FromRgb(148, 103, 189),
            System.Windows.Media.Color.FromRgb(140, 86, 75),
            System.Windows.Media.Color.FromRgb(227, 119, 194),
            System.Windows.Media.Color.FromRgb(127, 127, 127),
            System.Windows.Media.Color.FromRgb(188, 189, 34),
            System.Windows.Media.Color.FromRgb(23, 190, 207)
        };

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            var margin = Math.Max(ExternalRadius * 1.35f, 0.1f);
            return (new Luminy.Model.Range(-margin, margin), new Luminy.Model.Range(-margin, margin));
        }

        public override IEnumerable<(string Label, System.Windows.Media.Color Color)> GetLegendItems(System.Windows.Media.Color defaultColor)
        {
            if (Values == null || Labels == null) yield break;

            var count = Math.Min(Values.Length, Labels.Length);
            for (var i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(Labels[i]))
                {
                    yield return (Labels[i], SlicePalette[i % SlicePalette.Length]);
                }
            }
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (Values == null || Values.Length == 0) return;

            var validIndices = new List<int>();
            var total = 0f;
            for (var i = 0; i < Values.Length; i++)
            {
                if (float.IsFinite(Values[i]) && Values[i] > 0)
                {
                    validIndices.Add(i);
                    total += Values[i];
                }
            }

            if (total <= 0 || validIndices.Count == 0) return;

            var (cx, cy) = map(0, 0);
            var (rx, _) = map(ExternalRadius, 0);
            var (_, ry) = map(0, ExternalRadius);
            var rExt = Math.Min(Math.Abs(rx - cx), Math.Abs(ry - cy));
            var rInt = ExternalRadius > 0
                ? rExt * (Math.Clamp(InternalRadius, 0f, ExternalRadius) / ExternalRadius)
                : 0f;

            var currentAngle = -MathF.PI / 2f;
            var whitePen = new Pen(Brushes.White, 1.5);
            whitePen.Freeze();

            var culture = CultureInfo.InvariantCulture;
            var typeface = new Typeface("Segoe UI");

            foreach (var index in validIndices)
            {
                var val = Values[index];
                var ratio = val / total;
                var sweepAngle = ratio * MathF.PI * 2f;
                var startAngle = currentAngle;
                var endAngle = currentAngle + sweepAngle;
                currentAngle = endAngle;

                var sliceColor = SlicePalette[index % SlicePalette.Length];
                var brush = new SolidColorBrush(sliceColor);
                brush.Freeze();

                // Build sector geometry
                var geom = new StreamGeometry();
                using (var ctx = geom.Open())
                {
                    var p1Ext = new Point(cx + rExt * MathF.Cos(startAngle), cy + rExt * MathF.Sin(startAngle));
                    var p2Ext = new Point(cx + rExt * MathF.Cos(endAngle), cy + rExt * MathF.Sin(endAngle));
                    var isLargeArc = sweepAngle > MathF.PI;

                    ctx.BeginFigure(p1Ext, isFilled: true, isClosed: true);
                    ctx.ArcTo(p2Ext, new Size(rExt, rExt), 0, isLargeArc, SweepDirection.Clockwise, isStroked: true, isSmoothJoin: true);

                    if (rInt <= 0.001f)
                    {
                        ctx.LineTo(new Point(cx, cy), isStroked: true, isSmoothJoin: false);
                    }
                    else
                    {
                        var p2Int = new Point(cx + rInt * MathF.Cos(endAngle), cy + rInt * MathF.Sin(endAngle));
                        var p1Int = new Point(cx + rInt * MathF.Cos(startAngle), cy + rInt * MathF.Sin(startAngle));

                        ctx.LineTo(p2Int, isStroked: true, isSmoothJoin: false);
                        ctx.ArcTo(p1Int, new Size(rInt, rInt), 0, isLargeArc, SweepDirection.Counterclockwise, isStroked: true, isSmoothJoin: true);
                    }
                }

                geom.Freeze();
                dc.DrawGeometry(brush, whitePen, geom);

                var midAngle = startAngle + sweepAngle / 2f;

                // Inside text
                if (InsideValueDisplay != DonutValueDisplay.None && ratio >= 0.05f)
                {
                    var rMid = (rInt + rExt) / 2f;
                    var ix = cx + rMid * MathF.Cos(midAngle);
                    var iy = cy + rMid * MathF.Sin(midAngle);

                    var textContent = InsideValueDisplay == DonutValueDisplay.Percent
                        ? $"{(ratio * 100f):0.#}%"
                        : val.ToString("0.##", culture);

                    var ft = new FormattedText(textContent, culture, FlowDirection.LeftToRight, typeface, 11, Brushes.White, 1.0);
                    dc.DrawText(ft, new Point(ix - ft.Width / 2, iy - ft.Height / 2));
                }

                // Outside labels
                if (Labels != null && index < Labels.Length && !string.IsNullOrEmpty(Labels[index]))
                {
                    var rLbl = rExt * 1.10f;
                    var lx = cx + rLbl * MathF.Cos(midAngle);
                    var ly = cy + rLbl * MathF.Sin(midAngle);

                    var ft = new FormattedText(Labels[index], culture, FlowDirection.LeftToRight, typeface, 11, Brushes.Black, 1.0);
                    var cosMid = MathF.Cos(midAngle);
                    var xOffset = cosMid switch
                    {
                        > 0.3f => 0,
                        < -0.3f => -ft.Width,
                        _ => -ft.Width / 2
                    };

                    dc.DrawText(ft, new Point(lx + xOffset, ly - ft.Height / 2));
                }
            }
        }
    }
}
