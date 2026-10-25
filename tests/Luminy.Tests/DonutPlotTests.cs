using System.IO;
using System.Linq;
using Luminy.Builders;
using Luminy.Model;

namespace Luminy.Tests
{
    [TestFixture]
    public class DonutPlotTests
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
        public void DonutPlot_GetValueRange_ReturnsSymmetricRange()
        {
            var values = new[] { 30f, 50f, 20f };
            var labels = new[] { "A", "B", "C" };
            var donut = new DonutPlot(values, labels, ExternalRadius: 2.0f, InternalRadius: 1.0f);

            var (xRange, yRange) = donut.GetValueRange();

            Assert.That(xRange.Min, Is.EqualTo(-xRange.Max));
            Assert.That(yRange.Min, Is.EqualTo(-yRange.Max));
            Assert.That(xRange.Max, Is.GreaterThan(2.0f));
        }

        [Test]
        public void DonutPlot_GetLegendItems_ReturnsEntriesForNonEmptyLabels()
        {
            var values = new[] { 10f, 20f, 30f };
            var labels = new[] { "Alpha", "", "Gamma" };
            var donut = new DonutPlot(values, labels);

            var items = donut.GetLegendItems().ToList();

            Assert.That(items.Count, Is.EqualTo(2));
            Assert.That(items[0].Label, Is.EqualTo("Alpha"));
            Assert.That(items[1].Label, Is.EqualTo("Gamma"));
        }

        [Test]
        public void DonutPlot_InsideValues_ThresholdHidesSmallSlices()
        {
            // 90%, 8%, 2% -> with 5% threshold, 2% slice value must be hidden
            var values = new[] { 90f, 8f, 2f };
            var labels = new[] { "Big", "Medium", "Tiny" };
            var luminy = new LuminyBuilder();

            var donut = luminy.Donut(
                values,
                labels,
                insideValueDisplay: DonutValueDisplay.Percent,
                minInsideValueThreshold: 0.05f);

            var display = luminy.DonutDisplay(donut);
            var svg = display.ToSvg();

            Assert.That(svg, Does.Contain("90%"));
            Assert.That(svg, Does.Contain("8%"));
            Assert.That(svg, Does.Not.Contain("2%"));
        }

        [Test]
        public void DonutPlot_PieChartMode_WhenInternalRadiusZero()
        {
            var values = new[] { 25f, 75f };
            var labels = new[] { "Q1", "Rest" };
            var luminy = new LuminyBuilder();

            var pie = luminy.Donut(values, labels, internalRadius: 0f);
            var display = luminy.DonutDisplay(pie);
            var svg = display.ToSvg();

            Assert.That(svg, Does.Contain("<path"));
            Assert.That(svg, Does.Contain("Q1"));
            Assert.That(svg, Does.Contain("Rest"));
        }

        [Test]
        public void GenerateDonutShowcaseReport()
        {
            var luminy = new LuminyBuilder();

            var values = new[] { 45f, 25f, 18f, 10f, 2f };
            var labels = new[] { "Desktop", "Mobile", "Tablet", "Console", "Other" };

            // 1. Classic Donut with slice labels and legend
            var classicDonut = luminy.DonutDisplay(
                values,
                labels,
                title: "Platform Market Share",
                insideValueDisplay: DonutValueDisplay.Percent,
                minInsideValueThreshold: 0.04f);

            // 2. Pure Pie Chart (InternalRadius = 0)
            var pieChart = luminy.DonutDisplay(
                values,
                labels,
                title: "Platform Share (Pie Chart)",
                internalRadius: 0f,
                insideValueDisplay: DonutValueDisplay.Percent);

            // 3. Raw Values inside slices
            var rawDonut = luminy.DonutDisplay(
                values,
                labels,
                title: "Units Sold (Raw Values)",
                insideValueDisplay: DonutValueDisplay.Raw);

            // 4. Concentric Donuts: 2025 (inner) vs 2026 (outer)
            var inner2025 = luminy.Donut(
                new[] { 50f, 30f, 20f },
                new[] { "Product A", "Product B", "Product C" },
                externalRadius: 0.55f,
                internalRadius: 0.30f,
                showSliceLabels: false,
                insideValueDisplay: DonutValueDisplay.Percent);

            var outer2026 = luminy.Donut(
                new[] { 40f, 40f, 20f },
                new[] { "Product A (2026)", "Product B (2026)", "Product C (2026)" },
                externalRadius: 0.95f,
                internalRadius: 0.65f,
                showSliceLabels: true,
                insideValueDisplay: DonutValueDisplay.Percent);

            var concentricDisplay = new Display(
                Plots: System.Collections.Immutable.ImmutableArray.Create<Plot>(inner2025, outer2026),
                Width: 600,
                Height: 400,
                Title: "Year-over-Year Concentric Comparison",
                AxesStyle: AxesStyle.None,
                ShowGrid: false,
                ShowLegend: true,
                LegendPosition: LegendPosition.TopRight
            );

            var report = new ReportBuilder(reportsPath, "donut_showcase.html")
                .AddTitle("Luminy Donut & Pie Chart Showcase")
                .AddSection("Classic Donut Chart")
                .AddParagraph("Rendered with 60% inner hole, external labels at 110% radius, and inside percentage badges.")
                .AddChart(new Chart(reportsPath, classicDonut), "Platform Share (Donut)")
                .AddSection("Alternative Variations")
                .AddParagraph("Side-by-side comparison of full Pie chart, raw values, and concentric multi-series donuts.")
                .AddChart(new Chart(reportsPath, pieChart, rawDonut) { Columns = 2 })
                .AddSection("Concentric Donuts (Multi-Series)")
                .AddParagraph("Demonstrates multiple concentric rings in the same Display, comparing two generations of data.")
                .AddChart(new Chart(reportsPath, concentricDisplay), "Inner Ring (2025) vs Outer Ring (2026)")
                .Build();

            Assert.Pass($"Donut showcase report generated at {Path.Combine(reportsPath, "donut_showcase.html")}");
        }
    }
}
