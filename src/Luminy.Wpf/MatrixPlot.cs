using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Luminy.Model;

namespace Luminy.Wpf
{
    /// <summary>
    /// Renders a 2D array of data as a color-coded matrix (heatmap) using DirectX DrawingContext.
    /// </summary>
    public class MatrixPlot : PlotElement
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(float[,]), typeof(MatrixPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty JaggedDataProperty =
            DependencyProperty.Register(nameof(JaggedData), typeof(float[][]), typeof(MatrixPlot),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ColorMinProperty =
            DependencyProperty.Register(nameof(ColorMin), typeof(System.Windows.Media.Color), typeof(MatrixPlot),
                new FrameworkPropertyMetadata(System.Windows.Media.Color.FromRgb(240, 249, 255), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ColorMaxProperty =
            DependencyProperty.Register(nameof(ColorMax), typeof(System.Windows.Media.Color), typeof(MatrixPlot),
                new FrameworkPropertyMetadata(System.Windows.Media.Color.FromRgb(2, 132, 199), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CellStrokeProperty =
            DependencyProperty.Register(nameof(CellStroke), typeof(System.Windows.Media.Color?), typeof(MatrixPlot),
                new FrameworkPropertyMetadata(System.Windows.Media.Color.FromArgb(100, 255, 255, 255), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary> Gets or sets the 2D numerical data matrix. </summary>
        public float[,]? Data
        {
            get => (float[,]?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary> Gets or sets the jagged numerical data array. </summary>
        public float[][]? JaggedData
        {
            get => (float[][]?)GetValue(JaggedDataProperty);
            set => SetValue(JaggedDataProperty, value);
        }

        /// <summary> Gets or sets the color mapped to the minimum value. </summary>
        public System.Windows.Media.Color ColorMin
        {
            get => (System.Windows.Media.Color)GetValue(ColorMinProperty);
            set => SetValue(ColorMinProperty, value);
        }

        /// <summary> Gets or sets the color mapped to the maximum value. </summary>
        public System.Windows.Media.Color ColorMax
        {
            get => (System.Windows.Media.Color)GetValue(ColorMaxProperty);
            set => SetValue(ColorMaxProperty, value);
        }

        /// <summary> Gets or sets the outline stroke color for matrix cells. </summary>
        public System.Windows.Media.Color? CellStroke
        {
            get => (System.Windows.Media.Color?)GetValue(CellStrokeProperty);
            set => SetValue(CellStrokeProperty, value);
        }

        public override (Luminy.Model.Range xRange, Luminy.Model.Range yRange) GetValueRange()
        {
            if (!TryGetMatrix(out var rows, out var cols, out _))
                return (new Luminy.Model.Range(0, 0), new Luminy.Model.Range(0, 0));

            return (
                new Luminy.Model.Range(0.5f, cols + 0.5f),
                new Luminy.Model.Range(0.5f, rows + 0.5f)
            );
        }

        public override void Render(DrawingContext dc, CoordinateMap map, Rect plotBounds, System.Windows.Media.Color defaultColor)
        {
            if (!TryGetMatrix(out var rows, out var cols, out var getVal)) return;

            // Compute min/max
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    var val = getVal(i, j);
                    if (!float.IsFinite(val)) continue;
                    if (val < min) min = val;
                    if (val > max) max = val;
                }
            }

            if (min > max) return;
            var span = Math.Abs(max - min);

            Pen? cellPen = null;
            if (CellStroke.HasValue && CellStroke.Value.A > 0)
            {
                var strokeBrush = new SolidColorBrush(CellStroke.Value);
                strokeBrush.Freeze();
                cellPen = new Pen(strokeBrush, 0.5);
                cellPen.Freeze();
            }

            // Precompute palette of 256 colors for fast lookup
            const int paletteSize = 256;
            var paletteBrushes = new SolidColorBrush[paletteSize];
            for (var p = 0; p < paletteSize; p++)
            {
                var t = (float)p / (paletteSize - 1);
                var c = LerpColor(ColorMin, ColorMax, t);
                var b = new SolidColorBrush(c);
                b.Freeze();
                paletteBrushes[p] = b;
            }

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < cols; j++)
                {
                    var val = getVal(i, j);
                    if (!float.IsFinite(val)) continue;

                    var (left, top) = map(j + 0.5f, i + 0.5f);
                    var (right, bottom) = map(j + 1.5f, i + 1.5f);

                    var rx = Math.Min(left, right);
                    var ry = Math.Min(top, bottom);
                    var rw = Math.Abs(right - left);
                    var rh = Math.Abs(bottom - top);

                    var normalized = span < 1e-12f ? 0.5f : Math.Clamp((val - min) / span, 0f, 1f);
                    var paletteIndex = Math.Clamp((int)(normalized * (paletteSize - 1)), 0, paletteSize - 1);
                    var brush = paletteBrushes[paletteIndex];

                    dc.DrawRectangle(brush, cellPen, new Rect(rx, ry, rw, rh));
                }
            }
        }

        private bool TryGetMatrix(out int rows, out int cols, out Func<int, int, float> getVal)
        {
            if (Data != null && Data.Length > 0)
            {
                rows = Data.GetLength(0);
                cols = Data.GetLength(1);
                var data = Data;
                getVal = (r, c) => data[r, c];
                return rows > 0 && cols > 0;
            }

            if (JaggedData != null && JaggedData.Length > 0)
            {
                rows = JaggedData.Length;
                cols = JaggedData.Max(r => r?.Length ?? 0);
                var jagged = JaggedData;
                getVal = (r, c) => (jagged[r] != null && c < jagged[r].Length) ? jagged[r][c] : float.NaN;
                return rows > 0 && cols > 0;
            }

            rows = 0;
            cols = 0;
            getVal = (_, _) => float.NaN;
            return false;
        }

        private static System.Windows.Media.Color LerpColor(System.Windows.Media.Color a, System.Windows.Media.Color b, float t)
        {
            var r = (byte)(a.R + (b.R - a.R) * t);
            var g = (byte)(a.G + (b.G - a.G) * t);
            var bVal = (byte)(a.B + (b.B - a.B) * t);
            var alpha = (byte)(a.A + (b.A - a.A) * t);
            return System.Windows.Media.Color.FromArgb(alpha, r, g, bVal);
        }
    }
}
