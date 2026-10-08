using Luminy.Model;
using System.Collections.Immutable;
using System.Linq;

namespace Luminy.Builders
{
    /// <summary>
    /// A stateful builder for creating immutable Luminy visualizations.
    /// Manages defaults, color palettes, and data processing.
    /// </summary>
    public class LuminyBuilder
    {
        // --- Builder Settings (The "Twist") ---

        /// <summary> Default width for generated displays. </summary>
        public float Width { get; set; } = 600;

        /// <summary> Default height for generated displays. </summary>
        public float Height { get; set; } = 400;

        /// <summary> Default padding for generated displays. </summary>
        public float Padding { get; set; } = 40;

        /// <summary> Default inset for generated displays. </summary>
        public int Inset { get; set; } = 20;

        /// <summary> Default grid color. </summary>
        public Color GridColor { get; set; } = new(0.9f, 0.9f, 0.9f);

        /// <summary> Whether to show the grid by default. </summary>
        public bool ShowGrid { get; set; } = false;

        /// <summary> Whether to show the legend by default. </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary> Default legend position. </summary>
        public LegendPosition LegendPosition { get; set; } = LegendPosition.TopRight;

        /// <summary> Default axes style. </summary>
        public AxesStyle AxesStyle { get; set; } = AxesStyle.Frame;

        // --- Color Management ---

        private int colorIndex = 0;
        private static readonly Color[] Palette = 
        {
            new (0.12f, 0.47f, 0.71f), // Blue
            new (1.00f, 0.50f, 0.05f), // Orange
            new (0.17f, 0.63f, 0.17f), // Green
            new (0.84f, 0.15f, 0.16f), // Red
            new (0.58f, 0.40f, 0.74f), // Purple
            new (0.55f, 0.34f, 0.29f), // Brown
            new (0.89f, 0.47f, 0.76f), // Pink
            new (0.50f, 0.50f, 0.50f), // Gray
            new (0.74f, 0.74f, 0.13f), // Yellow-Green
            new (0.09f, 0.75f, 0.81f)  // Cyan
        };

        /// <summary>
        /// Resets the color palette index to zero.
        /// </summary>
        public void ResetPalette() => colorIndex = 0;

        /// <summary>
        /// Returns the next color in the palette.
        /// </summary>
        public Color NextColor() => Palette[colorIndex++ % Palette.Length];

        // --- Plot Factory Methods ---

        /// <summary> Creates a line plot. </summary>
        public LinePlot Line(float[] x, float[] y, string label = "", Color? color = null, int strokeWidth = 2)
        {
            return new LinePlot(x, y, label, color ?? NextColor(), strokeWidth);
        }

        /// <summary> Creates a point (scatter) plot. Handles non-finite data points. </summary>
        public PointPlot Points(float[] x, float[] y, string label = "", Color? color = null, int pointSize = 3)
        {
            var (cleanX, cleanY) = PointPlot.CleanData(x, y);

            return new PointPlot(cleanX, cleanY, label, color ?? NextColor(), pointSize);
        }

        /// <summary> Creates a bar chart with equally spaced bars. </summary>
        public BarChart Bars(float[] heights, string label = "", Color? color = null, float? barWidth = null)
        {
            var x = Enumerable.Range(0, heights.Length).Select(i => (float)i).ToArray();
            return Bars(x, heights, label, color, barWidth);
        }

        /// <summary> Creates a bar chart with explicit coordinates. </summary>
        public BarChart Bars(float[] x, float[] heights, string label = "", Color? color = null, float? barWidth = null)
        {
            var width = barWidth ?? BarChart.CalculateDefaultWidth(x);
            return new BarChart(x, heights, width, label, color ?? NextColor());
        }

        /// <summary> Creates a statistical box plot. </summary>
        public BoxPlot Box(float[] data, float x = 0, string label = "", Color? color = null, float width = 0.8f)
        {
            return new BoxPlot(data, x, label, color ?? NextColor(), width);
        }

        /// <summary> Creates a histogram plot. Handles non-finite data points. </summary>
        public HistogramPlot Histogram(float[] data, int binCount = 10, string label = "", Color? color = null)
        {
            var filtered = data.Where(float.IsFinite).ToArray();
            return new HistogramPlot(filtered, binCount, label, color ?? NextColor());
        }

        /// <summary> Creates a heatmap matrix plot. </summary>
        public MatrixPlot Matrix(float[,] data, string label = "", Color? colorMin = null, Color? colorMax = null)
        {
            return new MatrixPlot(data, 
                colorMin ?? new Color(0.5f, 0.5f, 1f), 
                colorMax ?? new Color(1f, 0.1f, 0.1f), 
                label);
        }

        // --- Display Construction ---

        /// <summary>
        /// Combines one or more plots into an immutable Display object using the builder's current settings.
        /// </summary>
        public Display Display(params Plot[] plots)
        {
            return new Display(
                Plots: plots.ToImmutableArray(),
                Width: Width,
                Height: Height,
                Padding: Padding,
                Inset: Inset,
                AxesStyle: AxesStyle,
                ShowGrid: ShowGrid,
                GridColor: GridColor,
                ShowLegend: ShowLegend,
                LegendPosition: LegendPosition
            );
        }
    }
}