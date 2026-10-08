using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Specifies the visual style of the axes.
    /// </summary>
    public enum AxesStyle
    {
        /// <summary> Axes cross at (0,0). Ideal for mathematical functions. </summary>
        Normal,
        /// <summary> Axes on all sides forming a frame. Ideal for scientific plots. </summary>
        Frame,
        /// <summary> Frame style with ticks on all sides. </summary>
        Boxed,
        /// <summary> No axes or ticks are rendered. </summary>
        None
    }

    /// <summary>
    /// Specifies the scaling strategy for an axis.
    /// </summary>
    public enum ScaleType
    {
        /// <summary> Linear mapping between data and screen coordinates. </summary>
        Linear,
        /// <summary> Logarithmic (base e) mapping between data and screen coordinates. </summary>
        Logarithmic
    }

    /// <summary>
    /// Specifies where the legend should be placed.
    /// </summary>
    public enum LegendPosition
    {
        /// <summary> Top right corner inside the plot area. </summary>
        TopRight,
        /// <summary> Top left corner inside the plot area. </summary>
        TopLeft,
        /// <summary> Bottom right corner inside the plot area. </summary>
        BottomRight,
        /// <summary> Bottom left corner inside the plot area. </summary>
        BottomLeft
    }

    /// <summary>
    /// The central orchestrator for a visualization. Manages plots, scales, and axes rendering.
    /// This is an immutable record; use LuminyBuilder to create and configure it.
    /// </summary>
    public record Display(
        System.Collections.Immutable.ImmutableArray<Plot> Plots,
        float Width = 600,
        float Height = 400,
        float Padding = 40,
        string Title = "",
        string XLabel = "",
        string YLabel = "",
        int Inset = 20,
        AxesStyle AxesStyle = AxesStyle.Frame,
        bool ShowGrid = false,
        Color? GridColor = null,
        bool ShowLegend = true,
        LegendPosition LegendPosition = LegendPosition.TopRight,
        ScaleType XScaleType = ScaleType.Linear,
        ScaleType YScaleType = ScaleType.Linear,
        Range? XRange = null,
        Range? YRange = null
    )
    {
        /// <summary>
        /// Renders the visualization to an SVG string.
        /// </summary>
        /// <returns>A string containing the complete SVG XML.</returns>
        public string ToSvg()
        {
            var svg = new SvgBuilder();

            using (svg.OpenSvg(Width, Height))
            {
                // Calculate data ranges
                var ranges = Plots.Select(p => p.GetValueRange()).ToArray();
                var xDataRange = Range.Combine(ranges.Select(r => r.xRange).ToArray());
                var yDataRange = Range.Combine(ranges.Select(r => r.yRange).ToArray());

                // Use user-specified ranges if provided
                var xRange = XRange ?? xDataRange;
                var yRange = YRange ?? yDataRange;

                // Define clipping path
                var clipId = Guid.NewGuid().ToString("N");
                using (svg.OpenClipPath(clipId))
                {
                    svg.Rect(Padding, Padding, Width - 2f * Padding, Height - 2f * Padding);
                }

                // Create coordinate transformation function
                CoordinateMap coordinate = (u, v) =>
                {
                    // Transform u and v based on scale type
                    var scaledU = XScaleType == ScaleType.Logarithmic
                        ? (float.Log(u) - float.Log(xRange.Min)) / (float.Log(xRange.Max) - float.Log(xRange.Min))
                        : (u - xRange.Min) / (xRange.Max - xRange.Min);

                    var scaledV = YScaleType == ScaleType.Logarithmic
                        ? (float.Log(v) - float.Log(yRange.Min)) / (float.Log(yRange.Max) - float.Log(yRange.Min))
                        : (v - yRange.Min) / (yRange.Max - yRange.Min);

                    // Map to SVG coordinates
                    var x = Padding + Inset + scaledU * (Width - 2f * (Padding + Inset));
                    var y = Height - Padding - Inset - scaledV * (Height - 2f * (Padding + Inset));

                    return (x, y);
                };

                if (ShowGrid)
                {
                    DrawGrid(svg, xRange, yRange, coordinate);
                }

                // Render plots with clipping
                using (svg.OpenTag("g", ("clip-path", $"url(#{clipId})")))
                {
                    foreach (var plot in Plots)
                    {
                        plot.Render(svg, coordinate);
                    }
                }

                // Draw axes
                DrawAxes(svg, xRange, yRange, coordinate);

                // Draw title and labels
                DrawLabels(svg);

                // Draw legend if requested
                if (ShowLegend)
                {
                    DrawLegend(svg, xRange, yRange, coordinate);
                }
            }

            return svg.ToString();
        }

        private void DrawLabels(SvgBuilder svg)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                using (svg.OpenTag("text",
                           ("x", (Width / 2).ToString()),
                           ("y", (Padding / 2).ToString()),
                           ("text-anchor", "middle"),
                           ("font-size", "16"),
                           ("font-weight", "bold")))
                {
                    svg.Append(Title);
                }
            }

            if (!string.IsNullOrEmpty(XLabel))
            {
                using (svg.OpenTag("text",
                           ("x", (Width / 2).ToString()),
                           ("y", (Height - 5).ToString()),
                           ("text-anchor", "middle"),
                           ("font-size", "14")))
                {
                    svg.Append(XLabel);
                }
            }

            if (!string.IsNullOrEmpty(YLabel))
            {
                using (svg.OpenTag("text",
                           ("x", "15"),
                           ("y", (Height / 2).ToString()),
                           ("text-anchor", "middle"),
                           ("font-size", "14"),
                           ("transform", $"rotate(-90, 15, {Height / 2})")))
                {
                    svg.Append(YLabel);
                }
            }
        }

        private void DrawLegend(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            var labeledPlots = Plots.Where(p => !string.IsNullOrEmpty(p.Label)).ToArray();
            if (labeledPlots.Length == 0) return;

            const float rowHeight = 20;
            const float boxWidth = 15;
            const float padding = 10;
            float legendWidth = 100; // Minimal width, will be updated by content
            var legendHeight = labeledPlots.Length * rowHeight + padding * 2;

            // Estimate width (simplified, ideally we'd measure text)
            float maxLabelLength = labeledPlots.Max(p => p.Label.Length);
            legendWidth = Math.Max(legendWidth, maxLabelLength * 7 + boxWidth + padding * 3);

            // Determine position
            float lx, ly;
            var (plotLeft, plotTop) = coordinate(xRange.Min, yRange.Max);
            var (plotRight, plotBottom) = coordinate(xRange.Max, yRange.Min);

            plotLeft -= Inset;
            plotTop -= Inset;
            plotRight += Inset;
            plotBottom += Inset;

            switch (LegendPosition)
            {
                case LegendPosition.TopLeft:
                    lx = plotLeft + padding;
                    ly = plotTop + padding;
                    break;
                case LegendPosition.BottomRight:
                    lx = plotRight - legendWidth - padding;
                    ly = plotBottom - legendHeight - padding;
                    break;
                case LegendPosition.BottomLeft:
                    lx = plotLeft + padding;
                    ly = plotBottom - legendHeight - padding;
                    break;
                case LegendPosition.TopRight:
                default:
                    lx = plotRight - legendWidth - padding;
                    ly = plotTop + padding;
                    break;
            }

            // Draw legend box
            svg.Rect(lx, ly, legendWidth, legendHeight, Color.White, stroke: Color.Black, strokeWidth: 0.5f);

            // Draw series items
            for (var i = 0; i < labeledPlots.Length; i++)
            {
                var plot = labeledPlots[i];
                var iy = ly + padding + i * rowHeight + rowHeight / 2;

                // Color box/line
                svg.Rect(lx + padding, iy - boxWidth / 4, boxWidth, boxWidth / 2, plot.Color);

                // Label
                svg.Text(plot.Label, lx + padding + boxWidth + 5, iy + 4, "Arial, sans-serif", 10, "start", Color.Black);
            }
        }

        private void DrawAxes(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            switch (AxesStyle)
            {
                case AxesStyle.Normal:
                    DrawNormalAxes(svg, xRange, yRange, coordinate);
                    break;
                case AxesStyle.Frame:
                    DrawFrameAxes(svg, xRange, yRange, coordinate);
                    break;
                case AxesStyle.Boxed:
                    DrawBoxedAxes(svg, xRange, yRange, coordinate);
                    break;
                case AxesStyle.None:
                    // No axes
                    break;
            }
        }

        private void DrawNormalAxes(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // Check if (0,0) is within the visible range
            var zeroInXRange = xRange.Min <= 0 && xRange.Max >= 0;
            var zeroInYRange = yRange.Min <= 0 && yRange.Max >= 0;

            if (zeroInXRange && zeroInYRange)
            {
                // Draw axes crossing at (0,0)

                // X axis at y=0
                var (xStart, yZero) = coordinate(xRange.Min, 0);
                var (xEnd, _) = coordinate(xRange.Max, 0);
                svg.Line(xStart, yZero, xEnd, yZero, Color.Black, strokeWidth: 1);

                // Y axis at x=0
                var (xZero, yStart) = coordinate(0, yRange.Min);
                var (_, yEnd) = coordinate(0, yRange.Max);
                svg.Line(xZero, yStart, xZero, yEnd, Color.Black, strokeWidth: 1);

                // Draw ticks on these axes
                DrawNormalTicks(svg, xRange, yRange, coordinate);
            }
            else
            {
                // If (0,0) not visible, fall back to frame axes
                DrawFrameAxes(svg, xRange, yRange, coordinate);
            }
        }

        private void DrawNormalTicks(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // Calculate ticks that don't include 0 (we already have axes there)
            var xTicks = CalculateNiceTicks(xRange).Where(x => Math.Abs(x) > 0.001).ToArray();
            var yTicks = CalculateNiceTicks(yRange).Where(y => Math.Abs(y) > 0.001).ToArray();

            const float tickLength = 5f;

            // X ticks (on the X axis at y=0)
            foreach (var tickValue in xTicks)
            {
                var (tickX, tickY) = coordinate(tickValue, 0);

                // Draw tick mark (perpendicular to axis)
                svg.Line(tickX, tickY - tickLength, tickX, tickY + tickLength, Color.Black);

                // Draw label below axis
                svg.Text(
                    FormatTickLabel(tickValue, xRange),
                    tickX,
                    tickY + 15,  // Below the axis
                    "Arial, sans-serif", 10, "middle", Color.Black
                );
            }

            // Y ticks (on the Y axis at x=0)
            foreach (var tickValue in yTicks)
            {
                var (tickX, tickY) = coordinate(0, tickValue);

                // Draw tick mark
                svg.Line(tickX - tickLength, tickY, tickX + tickLength, tickY, Color.Black);

                // Draw label to the left of axis
                svg.Text(
                    FormatTickLabel(tickValue, yRange),
                    tickX - 5,
                    tickY + 3,  // Vertically centered
                    "Arial, sans-serif", 10, "end", Color.Black
                );
            }
        }

        private void DrawGrid(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            var gridColor = GridColor ?? new Color(0.9f, 0.9f, 0.9f);

            // Draw grid lines at tick positions
            var xTicks = CalculateTicks(xRange);
            var yTicks = CalculateTicks(yRange);

            foreach (var x in xTicks)
            {
                var (pxStart, pyStart) = coordinate(x, yRange.Min);
                var (pxEnd, pyEnd) = coordinate(x, yRange.Max);
                svg.Line(pxStart, pyStart, pxEnd, pyEnd, gridColor, 0.5f);
            }

            foreach (var y in yTicks)
            {
                var (pxStart, pyStart) = coordinate(xRange.Min, y);
                var (pxEnd, pyEnd) = coordinate(xRange.Max, y);
                svg.Line(pxStart, pyStart, pxEnd, pyEnd, gridColor, 0.5f);
            }
        }

        private List<float> CalculateTicks(Range range)
        {
            // Smart tick calculation (like matplotlib/Maple)
            var span = range.Max - range.Min;

            // Choose a "nice" step size
            var step = NiceStepSize(span / 5); // Aim for ~5 ticks

            var ticks = new List<float>();
            var start = Math.Ceiling(range.Min / step) * step;
            var end = Math.Floor(range.Max / step) * step;

            for (var tick = start; tick <= end; tick += step)
            {
                ticks.Add((float)tick);
            }

            return ticks;
        }

        private static double NiceStepSize(double roughStep)
        {
            // Find a "nice" number close to roughStep
            // Like: 1, 2, 5, 10, 20, 50, 100, etc.
            var exponent = Math.Floor(Math.Log10(roughStep));
            var fraction = roughStep / Math.Pow(10, exponent);

            // Nice fractions: 1, 2, 5, 10
            double[] niceFractions = { 1.0, 2.0, 5.0, 10.0 };
            var niceFraction = niceFractions.First(f => f >= fraction);

            return niceFraction * Math.Pow(10, exponent);
        }

        private void DrawFrameAxes(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // Frame: Axes on all four sides (full box around plot area)
            // Get the four corners of the plot area (inside padding)

            // Bottom-left corner
            var (left, bottom) = coordinate(xRange.Min, yRange.Min);

            bottom += Inset;
            left -= Inset;

            // Bottom-right corner  
            var (right, _) = coordinate(xRange.Max, yRange.Min);

            right += Inset;

            // Top-left corner
            var (_, top) = coordinate(xRange.Min, yRange.Max);

            top -= Inset;

            // Top-right corner (for completeness)
            var (right2, top2) = coordinate(xRange.Max, yRange.Max);

            right2 += Inset;
            top2 -= Inset;

            // Draw the frame (box around the plot)

            // 1. Bottom axis (X axis at yMin)
            svg.Line(left, bottom, right, bottom, Color.Black, strokeWidth: 1);

            // 2. Top axis (at yMax)
            svg.Line(left, top, right2, top2, Color.Black, strokeWidth: 1);

            // 3. Left axis (Y axis at xMin)
            svg.Line(left, bottom, left, top, Color.Black, strokeWidth: 1);

            // 4. Right axis (at xMax)
            svg.Line(right, bottom, right2, top2, Color.Black, strokeWidth: 1);

            // Draw ticks on bottom and left axes (inward ticks)
            DrawFrameTicks(svg, xRange, yRange, coordinate);
        }

        private void DrawFrameTicks(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // Calculate tick positions (smart ticks based on range)
            var xTicks = CalculateNiceTicks(xRange);
            var yTicks = CalculateNiceTicks(yRange);

            const float tickLength = 5f;

            // X ticks on bottom axis
            foreach (var tickValue in xTicks)
            {
                var (tickX, tickY) = coordinate(tickValue, yRange.Min);
                tickY += Inset;

                // Draw tick mark (upward from axis)
                svg.Line(tickX, tickY, tickX, tickY - tickLength, Color.Black);

                // Draw label below tick
                svg.Text(
                    content: FormatTickLabel(tickValue, xRange),
                    x: tickX,
                    y: tickY + 15,  // Below the axis
                    fontFamily: "Arial, sans-serif",
                    fontSize: 10,
                    textAnchor: "middle",
                    fill: Color.Black
                );
            }

            // Y ticks on left axis
            foreach (var tickValue in yTicks)
            {
                var (tickX, tickY) = coordinate(xRange.Min, tickValue);
                tickX -= Inset;

                // Draw tick mark (rightward from axis)
                svg.Line(tickX, tickY, tickX + tickLength, tickY, Color.Black);

                // Draw label to the left of tick
                svg.Text(
                    content: FormatTickLabel(tickValue, yRange),
                    x: tickX - 5,  // Left of the axis
                    y: tickY + 3,  // Vertically centered
                    fontFamily: "Arial, sans-serif",
                    fontSize: 10,
                    textAnchor: "end",  // Right-align
                    fill: Color.Black
                );
            }
        }

        private void DrawBoxedAxes(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // First draw the frame (same as Frame style)
            DrawFrameAxes(svg, xRange, yRange, coordinate);

            // Then add outward ticks on ALL sides
            DrawOutwardTicks(svg, xRange, yRange, coordinate);
        }

        private void DrawOutwardTicks(SvgBuilder svg, Range xRange, Range yRange, CoordinateMap coordinate)
        {
            // Calculate tick positions
            var xTicks = CalculateNiceTicks(xRange);
            var yTicks = CalculateNiceTicks(yRange);

            const float tickLength = 5f;

            // X ticks on BOTTOM (outward - downward)
            foreach (var tickValue in xTicks)
            {
                var (tickX, tickY) = coordinate(tickValue, yRange.Min);
                svg.Line(tickX, tickY, tickX, tickY + tickLength, Color.Black);  // Downward
            }

            // X ticks on TOP (outward - upward)
            foreach (var tickValue in xTicks)
            {
                var (tickX, tickY) = coordinate(tickValue, yRange.Max);
                svg.Line(tickX, tickY, tickX, tickY - tickLength, Color.Black);  // Upward
            }

            // Y ticks on LEFT (outward - leftward)
            foreach (var tickValue in yTicks)
            {
                var (tickX, tickY) = coordinate(xRange.Min, tickValue);
                svg.Line(tickX, tickY, tickX - tickLength, tickY, Color.Black);  // Leftward
            }

            // Y ticks on RIGHT (outward - rightward)
            foreach (var tickValue in yTicks)
            {
                var (tickX, tickY) = coordinate(xRange.Max, tickValue);
                svg.Line(tickX, tickY, tickX + tickLength, tickY, Color.Black);  // Rightward
            }

            // Labels (only on bottom and left for Boxed style)
            DrawFrameTicks(svg, xRange, yRange, coordinate);
        }

        // Helper methods for smart tick calculation
        private float[] CalculateNiceTicks(Range range)
        {
            // Note : nice ticks should apply to logarithmic scale


            // Implement "nice" tick algorithm (like in matplotlib/matlab)
            // This is a simplified version

            var span = range.Max - range.Min;
            if (span <= 0) return new[] { range.Min };

            // Find nice interval
            var exponent = double.Floor(double.Log10(span));
            var fraction = span / double.Pow(10, exponent);

            float niceFraction;
            if (fraction < 1.5) niceFraction = 1;
            else if (fraction < 3) niceFraction = 2;
            else if (fraction < 7) niceFraction = 5;
            else niceFraction = 10;

            var tickSpacing = niceFraction * double.Pow(10, exponent - 1);

            // Calculate starting tick (round up to nearest tickSpacing)
            var start = double.Ceiling(range.Min / tickSpacing) * tickSpacing;
            var end = double.Floor(range.Max / tickSpacing) * tickSpacing;

            // Generate ticks
            var ticks = new List<float>();
            for (var value = start; value <= end + tickSpacing * 0.5; value += tickSpacing)
            {
                // Mathematically enforce alignment to the tick grid to eliminate accumulated error
                var val = Math.Round(value / tickSpacing) * tickSpacing;
                
                if (val == 0) val = 0;          // avoids -0 in the display
                ticks.Add((float)val);
            }

            return ticks.ToArray();
        }

        private string FormatTickLabel(float value, Range range, ScaleType scaleType = ScaleType.Linear)
        {
            if (scaleType == ScaleType.Logarithmic)
            {
                // For log scales, we usually only want to label the 10^n marks
                var log = Math.Log10(value);
                if (Math.Abs(log - Math.Round(log)) < 1e-5)
                {
                    return $"10^{(int)Math.Round(log)}";
                }
                return ""; // Hide labels for minor ticks (2, 3, 4...) to avoid clutter
            }

            // Existing linear formatting...
            return value.ToString("G4", CultureInfo.InvariantCulture);
        }

        //private string FormatTickLabel(float value, Range range)
        //{
        //    if (value == 0) return "0";
        //    if (float.Abs(value) >= 1e4 || float.Abs(value) <= 1e-4) return value.ToString("e2");
        //    return value.ToString(CultureInfo.InvariantCulture);
        //}
    }
}