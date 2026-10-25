using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Luminy.Builders;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Retained-mode container orchestrating plot series, coordinate mappings, grids, and axes in WPF.
    /// </summary>
    public class Display : Panel
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Display),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty XLabelProperty =
            DependencyProperty.Register(nameof(XLabel), typeof(string), typeof(Display),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YLabelProperty =
            DependencyProperty.Register(nameof(YLabel), typeof(string), typeof(Display),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(Display),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(nameof(ShowLegend), typeof(bool), typeof(Display),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty AxesStyleProperty =
            DependencyProperty.Register(nameof(AxesStyle), typeof(AxesStyle), typeof(Display),
                new FrameworkPropertyMetadata(AxesStyle.Frame, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ChartPaddingProperty =
            DependencyProperty.Register(nameof(ChartPadding), typeof(double), typeof(Display),
                new FrameworkPropertyMetadata(40.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty InsetProperty =
            DependencyProperty.Register(nameof(Inset), typeof(int), typeof(Display),
                new FrameworkPropertyMetadata(20, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the chart title displayed at the top. </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary> Gets or sets the label for the X axis. </summary>
        public string XLabel
        {
            get => (string)GetValue(XLabelProperty);
            set => SetValue(XLabelProperty, value);
        }

        /// <summary> Gets or sets the label for the Y axis. </summary>
        public string YLabel
        {
            get => (string)GetValue(YLabelProperty);
            set => SetValue(YLabelProperty, value);
        }

        /// <summary> Gets or sets whether to display grid lines. </summary>
        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        /// <summary> Gets or sets whether to render the series legend. </summary>
        public bool ShowLegend
        {
            get => (bool)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        /// <summary> Gets or sets the visual style of the axes. </summary>
        public AxesStyle AxesStyle
        {
            get => (AxesStyle)GetValue(AxesStyleProperty);
            set => SetValue(AxesStyleProperty, value);
        }

        /// <summary> Gets or sets the outer padding around the chart in DIP. </summary>
        public double ChartPadding
        {
            get => (double)GetValue(ChartPaddingProperty);
            set => SetValue(ChartPaddingProperty, value);
        }

        /// <summary> Gets or sets the inner inset margin in DIP. </summary>
        public int Inset
        {
            get => (int)GetValue(InsetProperty);
            set => SetValue(InsetProperty, value);
        }

        private static readonly System.Windows.Media.Color[] Palette =
        {
            System.Windows.Media.Color.FromRgb(31, 119, 180),  // Blue
            System.Windows.Media.Color.FromRgb(255, 127, 14),  // Orange
            System.Windows.Media.Color.FromRgb(44, 160, 44),   // Green
            System.Windows.Media.Color.FromRgb(214, 39, 40),   // Red
            System.Windows.Media.Color.FromRgb(148, 103, 189), // Purple
            System.Windows.Media.Color.FromRgb(140, 86, 75),   // Brown
            System.Windows.Media.Color.FromRgb(227, 119, 194), // Pink
            System.Windows.Media.Color.FromRgb(127, 127, 127), // Gray
            System.Windows.Media.Color.FromRgb(188, 189, 34),  // Yellow-Green
            System.Windows.Media.Color.FromRgb(23, 190, 207)   // Cyan
        };

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
            }

            var w = double.IsInfinity(availableSize.Width) ? 600 : availableSize.Width;
            var h = double.IsInfinity(availableSize.Height) ? 400 : availableSize.Height;
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
            return finalSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var width = ActualWidth;
            var height = ActualHeight;
            if (width < 50 || height < 50) return;

            // 1. Calculate combined data range
            var plots = InternalChildren.OfType<PlotElement>().ToList();
            var ranges = plots.Select(p => p.GetValueRange()).ToList();

            var xRange = Luminy.Model.Range.Combine(ranges.Select(r => r.xRange).ToArray());
            var yRange = Luminy.Model.Range.Combine(ranges.Select(r => r.yRange).ToArray());

            if (xRange.Min == xRange.Max) xRange = new Luminy.Model.Range(xRange.Min - 1, xRange.Max + 1);
            if (yRange.Min == yRange.Max) yRange = new Luminy.Model.Range(yRange.Min - 1, yRange.Max + 1);

            // 2. Build coordinate mapping
            var padding = (float)ChartPadding;
            CoordinateMap coordinate = (u, v) =>
            {
                var scaledU = (u - xRange.Min) / (xRange.Max - xRange.Min);
                var scaledV = (v - yRange.Min) / (yRange.Max - yRange.Min);

                var x = padding + Inset + scaledU * (width - 2f * (padding + Inset));
                var y = height - padding - Inset - scaledV * (height - 2f * (padding + Inset));
                return ((float)x, (float)y);
            };

            var (plotLeft, plotTop) = coordinate(xRange.Min, yRange.Max);
            var (plotRight, plotBottom) = coordinate(xRange.Max, yRange.Min);

            var plotRect = new Rect(
                plotLeft - Inset,
                plotTop - Inset,
                Math.Max(0, plotRight - plotLeft + 2 * Inset),
                Math.Max(0, plotBottom - plotTop + 2 * Inset));

            var xTicks = CalculateNiceTicks(xRange);
            var yTicks = CalculateNiceTicks(yRange);

            // 3. Draw Grid
            if (ShowGrid && AxesStyle != AxesStyle.None)
            {
                var gridPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(229, 231, 235)), 1)
                {
                    DashStyle = DashStyles.Dash
                };
                gridPen.Freeze();

                foreach (var tick in xTicks)
                {
                    var (tx, _) = coordinate(tick, yRange.Min);
                    dc.DrawLine(gridPen, new Point(tx, plotRect.Top), new Point(tx, plotRect.Bottom));
                }

                foreach (var tick in yTicks)
                {
                    var (_, ty) = coordinate(xRange.Min, tick);
                    dc.DrawLine(gridPen, new Point(plotRect.Left, ty), new Point(plotRect.Right, ty));
                }
            }

            // 4. Render plots (with clipping)
            dc.PushClip(new RectangleGeometry(plotRect));
            for (var i = 0; i < plots.Count; i++)
            {
                var defaultColor = Palette[i % Palette.Length];
                plots[i].Render(dc, coordinate, plotRect, defaultColor);
            }
            dc.Pop();

            // 5. Draw Frame Axes & Ticks
            if (AxesStyle == AxesStyle.Frame)
            {
                var axisPen = new Pen(Brushes.Black, 1);
                axisPen.Freeze();

                dc.DrawRectangle(null, axisPen, plotRect);

                var typeface = new Typeface("Segoe UI");
                var culture = CultureInfo.InvariantCulture;

                // X Ticks & Labels
                foreach (var tick in xTicks)
                {
                    var (tx, _) = coordinate(tick, yRange.Min);
                    var ty = plotRect.Bottom;
                    dc.DrawLine(axisPen, new Point(tx, ty), new Point(tx, ty - 5));

                    var text = new FormattedText(
                        tick.ToString("G4", culture),
                        culture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        Brushes.Black,
                        1.0);
                    dc.DrawText(text, new Point(tx - text.Width / 2, ty + 5));
                }

                // Y Ticks & Labels
                foreach (var tick in yTicks)
                {
                    var (_, ty) = coordinate(xRange.Min, tick);
                    var tx = plotRect.Left;
                    dc.DrawLine(axisPen, new Point(tx, ty), new Point(tx + 5, ty));

                    var text = new FormattedText(
                        tick.ToString("G4", culture),
                        culture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        Brushes.Black,
                        1.0);
                    dc.DrawText(text, new Point(tx - text.Width - 6, ty - text.Height / 2));
                }
            }

            // 6. Draw Title & Labels
            DrawLabels(dc, width, height);

            // 7. Draw Legend
            if (ShowLegend)
            {
                DrawLegend(dc, plots, plotRect);
            }
        }

        private void DrawLabels(DrawingContext dc, double width, double height)
        {
            var culture = CultureInfo.InvariantCulture;
            var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

            if (!string.IsNullOrEmpty(Title))
            {
                var titleText = new FormattedText(Title, culture, FlowDirection.LeftToRight, typeface, 16, Brushes.Black, 1.0);
                dc.DrawText(titleText, new Point(width / 2 - titleText.Width / 2, ChartPadding / 2 - 5));
            }

            var labelTypeface = new Typeface("Segoe UI");
            if (!string.IsNullOrEmpty(XLabel))
            {
                var xText = new FormattedText(XLabel, culture, FlowDirection.LeftToRight, labelTypeface, 12, Brushes.DimGray, 1.0);
                dc.DrawText(xText, new Point(width / 2 - xText.Width / 2, height - ChartPadding / 2 - 5));
            }
        }

        private void DrawLegend(DrawingContext dc, List<PlotElement> plots, Rect plotRect)
        {
            var legendItems = new List<(string Label, System.Windows.Media.Color Color)>();
            for (var i = 0; i < plots.Count; i++)
            {
                var defaultColor = Palette[i % Palette.Length];
                legendItems.AddRange(plots[i].GetLegendItems(defaultColor));
            }

            if (legendItems.Count == 0) return;

            const double rowHeight = 20;
            const double boxWidth = 15;
            const double padding = 10;
            const double legendWidth = 130;
            var legendHeight = legendItems.Count * rowHeight + padding * 2;

            var lx = plotRect.Right - legendWidth - padding;
            var ly = plotRect.Top + padding;

            var legendRect = new Rect(lx, ly, legendWidth, legendHeight);
            dc.DrawRectangle(Brushes.White, new Pen(Brushes.LightGray, 1), legendRect);

            var typeface = new Typeface("Segoe UI");
            var culture = CultureInfo.InvariantCulture;

            for (var i = 0; i < legendItems.Count; i++)
            {
                var (lbl, clr) = legendItems[i];
                var iy = ly + padding + i * rowHeight + rowHeight / 2;

                var colorBrush = new SolidColorBrush(clr);
                colorBrush.Freeze();

                dc.DrawRectangle(colorBrush, null, new Rect(lx + padding, iy - boxWidth / 4, boxWidth, boxWidth / 2));

                var txt = new FormattedText(lbl, culture, FlowDirection.LeftToRight, typeface, 11, Brushes.Black, 1.0);
                dc.DrawText(txt, new Point(lx + padding + boxWidth + 6, iy - txt.Height / 2));
            }
        }

        private static float[] CalculateNiceTicks(Luminy.Model.Range range)
        {
            var span = range.Max - range.Min;
            if (span <= 0) return [range.Min];

            var exponent = Math.Floor(Math.Log10(span));
            var fraction = span / Math.Pow(10, exponent);

            float niceFraction = fraction switch
            {
                < 1.5 => 1,
                < 3.0 => 2,
                < 7.0 => 5,
                _ => 10
            };

            var tickSpacing = niceFraction * Math.Pow(10, exponent - 1);
            var start = Math.Ceiling(range.Min / tickSpacing) * tickSpacing;
            var end = Math.Floor(range.Max / tickSpacing) * tickSpacing;

            var ticks = new List<float>();
            for (var val = start; val <= end + tickSpacing * 0.5; val += tickSpacing)
            {
                var v = Math.Round(val / tickSpacing) * tickSpacing;
                if (Math.Abs(v) < 1e-12) v = 0;
                ticks.Add((float)v);
            }

            return [.. ticks];
        }
    }
}
