using System;
using System.IO;
using System.Linq;
using Luminy.Builders;
using Luminy.Model;

namespace Luminy.Tests
{
    [TestFixture]
    public class MultiSeriesTests
    {
        private readonly string reportsPath = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Reports"));

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(reportsPath);
        }

        [Test]
        public void GenerateMultiSeriesComparisonReport()
        {
            var x = Enumerable.Range(0, 100).Select(i => i * 0.1f).ToArray();
            
            // Multiple series with different frequencies
            var y1 = x.Select(v => MathF.Sin(v)).ToArray();
            var y2 = x.Select(v => MathF.Cos(v)).ToArray();
            var y3 = x.Select(v => MathF.Sin(v * 0.5f)).ToArray();
            var y4 = x.Select(v => MathF.Cos(v * 0.5f)).ToArray();

            var luminy = new LuminyBuilder { ShowGrid = true };

            var display = luminy.Display(
                luminy.Line(x, y1, "Sin(x)"),
                luminy.Line(x, y2, "Cos(x)"),
                luminy.Line(x, y3, "Sin(0.5x)"),
                luminy.Line(x, y4, "Cos(0.5x)")
            ) with {
                Title = "Trigonometric Comparison",
                XLabel = "Time",
                YLabel = "Amplitude"
            };

            var report = new ReportBuilder(reportsPath, "multi_series_test.html")
                .AddTitle("Multi-Series & Legend Test")
                .AddSection("Automatic Coloring")
                .AddParagraph("This plot demonstrates the automatic assignment of colors from the scientific palette and the rendering of a legend.")
                .AddChart(new Chart(reportsPath, display), "Four series with automatic colors")
                .AddSection("Legend Positioning")
                .AddParagraph("We can place the legend in different corners of the plot.")
                .AddChart(new Chart(reportsPath, 
                    display with { LegendPosition = LegendPosition.TopLeft, Title = "Top Left" },
                    display with { LegendPosition = LegendPosition.BottomLeft, Title = "Bottom Left" },
                    display with { LegendPosition = LegendPosition.BottomRight, Title = "Bottom Right" }
                ) { Columns = 3 })
                .Build();

            Assert.Pass($"Multi-series report generated at {Path.Combine(reportsPath, "multi_series_test.html")}");
        }
    }
}