using System;
using System.IO;
using Luminy.Builders;

namespace Luminy.Tests
{
    public class DebugTests
    {
        [Test]
        public void DebugHeatmap()
        {
            var heatmapData = new float[20, 30];
            for (var i = 0; i < 20; i++)
            for (var j = 0; j < 30; j++)
                heatmapData[i, j] = MathF.Sin(i * 0.3f) * MathF.Cos(j * 0.2f);

            var luminy = new LuminyBuilder();
            var plot = luminy.Matrix(heatmapData, label: "Debug Heatmap");
            var range = plot.GetValueRange();
            Console.WriteLine($"MatrixPlot range: X={range.xRange}, Y={range.yRange}");

            var display = luminy.Display(plot);

            var svg = display.ToSvg();
            Console.WriteLine($"SVG length: {svg.Length}");
            File.WriteAllText("heatmap_debug.svg", svg);
        }
    }
}