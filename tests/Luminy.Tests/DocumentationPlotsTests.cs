using System;
using System.IO;
using System.Linq;
using Luminy.Builders;
using Luminy.Model;

namespace Luminy.Tests
{
    /// <summary>
    /// Generates pure vector SVG visualizations for the official documentation catalog.
    /// </summary>
    [TestFixture]
    public class DocumentationPlotsTests
    {
        private readonly string reportsPath = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Reports"));

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(reportsPath);
        }

        private string SaveSvgWithGuid(Display display)
        {
            var svg = display.ToSvg();
            var fileName = $"{Guid.NewGuid()}.svg";
            var filePath = Path.Combine(reportsPath, fileName);
            File.WriteAllText(filePath, svg);
            TestContext.Out.WriteLine($"OUTPUT_SVG:{filePath}");
            return filePath;
        }

        /// <summary>
        /// Generates the official LinePlot documentation SVG.
        /// </summary>
        [Test]
        public void GenerateLinePlot_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 420, ShowGrid = true, ShowLegend = true };

            const int samples = 120;
            var x = new float[samples];
            var pureSin = new float[samples];
            var dampedCos = new float[samples];

            const float step = (float)(4.0 * Math.PI / samples);
            for (var i = 0; i < samples; i++)
            {
                var t = i * step;
                x[i] = t;
                pureSin[i] = MathF.Sin(2.0f * t);
                dampedCos[i] = MathF.Cos(2.0f * t) * MathF.Exp(-0.15f * t);
            }

            var line1 = luminy.Line(x, pureSin, label: "Harmonic Sine", color: new Color(0.01f, 0.52f, 0.78f), strokeWidth: 2);
            var line2 = luminy.Line(x, dampedCos, label: "Damped Cosine", color: new Color(0.98f, 0.45f, 0.09f), strokeWidth: 2);

            var display = luminy.Display(line1, line2) with
            {
                Title = "Harmonic Signal Analysis",
                XLabel = "Time t (s)",
                YLabel = "Signal Amplitude",
                ShowGrid = true,
                ShowLegend = true,
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official DonutPlot documentation SVG.
        /// </summary>
        [Test]
        public void GenerateDonutPlot_Svg()
        {
            var luminy = new LuminyBuilder();
            var values = new[] { 45f, 30f, 18f, 7f };
            var labels = new[] { "Desktop", "Mobile", "Tablet", "Console" };

            var display = luminy.DonutDisplay(
                values,
                labels,
                title: "Platform Market Share",
                internalRadius: 0.6f,
                externalRadius: 1.0f,
                insideValueDisplay: DonutValueDisplay.Percent,
                showSliceLabels: true
            ) with
            {
                Width = 700,
                Height = 460
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official PointPlot documentation SVG.
        /// </summary>
        [Test]
        public void GeneratePointPlot_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 420, ShowGrid = true, ShowLegend = true };

            const int count = 45;
            var xA = new float[count];
            var yA = new float[count];
            var xB = new float[count];
            var yB = new float[count];

            for (var i = 0; i < count; i++)
            {
                var u1 = (MathF.Sin(i * 12.9898f + 1f) * 43758.5453f) % 1f;
                var u2 = (MathF.Sin(i * 78.233f + 2f) * 43758.5453f) % 1f;
                xA[i] = 2.5f + (u1 - 0.5f) * 2.2f;
                yA[i] = 3.0f + (u2 - 0.5f) * 2.0f;

                var u3 = (MathF.Sin((i + 100) * 12.9898f + 3f) * 43758.5453f) % 1f;
                var u4 = (MathF.Sin((i + 100) * 78.233f + 4f) * 43758.5453f) % 1f;
                xB[i] = 6.2f + (u3 - 0.5f) * 2.5f;
                yB[i] = 5.8f + (u4 - 0.5f) * 2.2f;
            }

            var pointsA = luminy.Points(xA, yA, label: "Control Cohort", color: new Color(0.01f, 0.52f, 0.78f), pointSize: 4);
            var pointsB = luminy.Points(xB, yB, label: "Treated Cohort", color: new Color(0.98f, 0.45f, 0.09f), pointSize: 4);

            var display = luminy.Display(pointsA, pointsB) with
            {
                Title = "Bivariate Cluster Distribution",
                XLabel = "Feature X₁",
                YLabel = "Feature X₂",
                ShowGrid = true,
                ShowLegend = true,
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official BarChart documentation SVG.
        /// </summary>
        [Test]
        public void GenerateBarChart_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 420, ShowGrid = true, ShowLegend = true };

            var quarters = new[] { 1f, 2f, 3f, 4f, 5f, 6f };
            var earnings = new[] { 45f, 62f, -18f, 74f, 85f, 55f };

            var bars = luminy.Bars(quarters, earnings, label: "Net Earnings (k€)", color: new Color(0.05f, 0.58f, 0.53f), barWidth: 0.65f);

            var display = luminy.Display(bars) with
            {
                Title = "Quarterly Fiscal Results",
                XLabel = "Fiscal Period (Q1 - Q6)",
                YLabel = "Net Earnings (k€)",
                ShowGrid = true,
                ShowLegend = true,
                LegendPosition = LegendPosition.BottomRight,
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official HistogramPlot documentation SVG.
        /// </summary>
        [Test]
        public void GenerateHistogramPlot_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 420, ShowGrid = true, ShowLegend = true };

            const int samples = 500;
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var u1 = Math.Max(1e-6f, Math.Abs((MathF.Sin(i * 12.9898f + 5f) * 43758.5453f) % 1f));
                var u2 = (MathF.Sin(i * 78.233f + 11f) * 43758.5453f) % 1f;
                var r = MathF.Sqrt(-2.0f * MathF.Log(u1));
                var theta = 2.0f * MathF.PI * u2;
                var z0 = r * MathF.Cos(theta);

                var center = (i % 2 == 0) ? 6.0f : 14.0f;
                data[i] = center + z0 * 1.8f;
            }

            var hist = luminy.Histogram(data, binCount: 18, label: "Sample Density", color: new Color(0.55f, 0.36f, 0.96f));

            var display = luminy.Display(hist) with
            {
                Title = "Bimodal Frequency Distribution",
                XLabel = "Measurement Variable (x)",
                YLabel = "Frequency Count",
                ShowGrid = true,
                ShowLegend = true,
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official BoxPlot documentation SVG.
        /// </summary>
        [Test]
        public void GenerateBoxPlot_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 420, ShowGrid = true, ShowLegend = true };

            float[] control = [10.2f, 10.8f, 11.1f, 9.7f, 10.5f, 11.4f, 10.3f, 9.9f, 10.6f, 14.5f];
            float[] dose10 = [11.5f, 11.9f, 12.2f, 11.0f, 12.4f, 11.8f, 12.1f, 11.6f, 12.3f, 8.8f];
            float[] dose25 = [12.8f, 13.4f, 13.1f, 12.6f, 13.7f, 13.2f, 13.5f, 12.9f, 13.3f, 13.0f];
            float[] dose50 = [14.2f, 14.9f, 14.5f, 14.0f, 15.1f, 14.6f, 14.8f, 14.3f, 15.3f, 14.7f];
            float[] dose100 = [16.0f, 16.8f, 16.5f, 15.9f, 17.2f, 16.6f, 17.0f, 16.3f, 17.4f, 19.2f];

            var box1 = luminy.Box(control, x: 1f, label: "Placebo Control", color: new Color(0.01f, 0.52f, 0.78f), width: 0.5f);
            var box2 = luminy.Box(dose10, x: 2f, label: "Low Dose 10mg", color: new Color(0.05f, 0.65f, 0.91f), width: 0.5f);
            var box3 = luminy.Box(dose25, x: 3f, label: "Medium Dose 25mg", color: new Color(0.06f, 0.73f, 0.51f), width: 0.5f);
            var box4 = luminy.Box(dose50, x: 4f, label: "High Dose 50mg", color: new Color(0.96f, 0.62f, 0.04f), width: 0.5f);
            var box5 = luminy.Box(dose100, x: 5f, label: "Max Dose 100mg", color: new Color(0.94f, 0.27f, 0.27f), width: 0.5f);

            var display = luminy.Display(box1, box2, box3, box4, box5) with
            {
                Title = "Clinical Biomarker Response (Tukey Boxplot)",
                XLabel = "Study Cohort (1: Placebo, 2: 10mg, 3: 25mg, 4: 50mg, 5: 100mg)",
                YLabel = "Biomarker Level (mg/dL)",
                ShowGrid = true,
                ShowLegend = true,
                LegendPosition = LegendPosition.BottomRight,
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }

        /// <summary>
        /// Generates the official MatrixPlot documentation SVG.
        /// </summary>
        [Test]
        public void GenerateMatrixPlot_Svg()
        {
            var luminy = new LuminyBuilder { Width = 700, Height = 460, ShowGrid = false, ShowLegend = false };

            const int size = 12;
            var matrix = new float[size, size];
            const float center = (size - 1) / 2.0f;

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var di = (i - center) / center;
                    var dj = (j - center) / center;
                    var dist = MathF.Sqrt(di * di + dj * dj);
                    matrix[i, j] = MathF.Cos(dist * 2.5f * MathF.PI) * MathF.Exp(-dist * 0.7f);
                }
            }

            var heatmap = luminy.Matrix(
                matrix,
                label: "Thermal Intensity",
                colorMin: new Color(0.94f, 0.98f, 1.0f),
                colorMax: new Color(0.01f, 0.52f, 0.78f)
            );

            var display = luminy.Display(heatmap) with
            {
                Title = "2D Intensity Ripple Field",
                XLabel = "Matrix Column (j)",
                YLabel = "Matrix Row (i)",
                AxesStyle = AxesStyle.Frame
            };

            var path = SaveSvgWithGuid(display);
            Assert.That(File.Exists(path), Is.True);
        }
    }
}
