using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminy.Builders;
using Luminy.Model;

namespace Luminy.Tests
{
    [TestFixture]
    [Category("Demo")]
    public class SvdVisualizationTests
    {
        private readonly string reportsPath = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "Reports"));

        [SetUp]
        public void Setup()
        {
            // Ensure reports directory exists
            Directory.CreateDirectory(reportsPath);
        }

        [Test]
        public void GenerateSingularValueDecayReport()
        {
            // Generate synthetic singular values (typical decay pattern)
            var singularValues = GenerateSingularValues(count: 256, decayRate: 0.10f);

            // Create the plot
            var display = CreateSingularValueDisplay(singularValues);

            // Build HTML report
            var report = new ReportBuilder(reportsPath, "singular_value_decay.html")
                .AddTitle("Singular Value Decay Analysis")
                .AddSection("Mathematical Background")
                .AddParagraph("Singular Value Decomposition (SVD) decomposes a matrix M into UΣVᵀ where Σ contains singular values on the diagonal. The decay rate of these values indicates the compressibility of the matrix.")
                .AddParagraph("Fast decay → Highly compressible")
                .AddParagraph("Slow decay → Difficult to compress")
                .AddSection("Simulated Data")
                .AddParagraph($"Generated {singularValues.Length} singular values with exponential decay.")
                .AddCodeBlock("// Generation formula:\nσ_i = exp(-decay_rate * i) * noise(i)", "csharp")
                .AddSection("Visualization")
                .AddChart(new Chart(reportsPath, display), "Singular value spectrum with exponential decay")
                .AddSection("Statistics")
                .AddTable(new[,] {
                    { "Total singular values", singularValues.Length.ToString() },
                    { "Sum (Frobenius norm)", singularValues.Sum().ToString("F4") },
                    { "Largest (σ₁)", singularValues[0].ToString("F4") },
                    { "Smallest (σₙ)", singularValues[^1].ToString("F4") },
                    { "Decay ratio (σ₁/σₙ)", (singularValues[0] / singularValues[^1]).ToString("F2") }
                })
                .Build();


            Assert.Pass("Report generated successfully");
        }

        [Test]
        public void GenerateCompressionAnalysisReport()
        {
            // Simulate compression at different ranks
            var ranks = Enumerable.Range(1, 64).Select(i => i * 4).ToArray();
            var compressionRatios = ranks.Select(r => 100f / r).ToArray();
            var energyCaptured = ranks.Select(r => 1f - MathF.Exp(-0.02f * r)).ToArray();
            var reconstructionErrors = ranks.Select(r => 0.1f / MathF.Sqrt(r)).ToArray();

            var luminy = new LuminyBuilder { Width = 600, Height = 400 };

            var compressionPlot = luminy.Display(
                luminy.Line(ranks.Select(r => (float)r).ToArray(), compressionRatios, color: new Color(0.8f, 0.2f, 0.2f))
            ) with {
                Title = "Compression Ratio vs Rank",
                XLabel = "Rank (r)",
                YLabel = "Compression Ratio"
            };

            var energyPlot = luminy.Display(
                luminy.Line(ranks.Select(r => (float)r).ToArray(), energyCaptured, color: new Color(0.2f, 0.6f, 0.2f))
            ) with {
                Title = "Energy Captured vs Rank",
                XLabel = "Rank (r)",
                YLabel = "Fraction of Energy"
            };

            var errorPlot = luminy.Display(
                luminy.Line(ranks.Select(r => (float)r).ToArray(), reconstructionErrors, color: new Color(0.2f, 0.2f, 0.8f))
            ) with {
                Title = "Reconstruction Error vs Rank",
                XLabel = "Rank (r)",
                YLabel = "Mean Squared Error",
                YScaleType = ScaleType.Logarithmic
            };

            // Build comprehensive report
            var report = new ReportBuilder(reportsPath, "compression_analysis.html")
                .AddTitle("SVD Compression Trade-off Analysis")
                .AddSection("Introduction")
                .AddParagraph("When compressing a matrix using SVD with rank r, we face trade-offs between compression ratio, energy preservation, and reconstruction error.")
                .AddSection("Trade-off Visualization")
                .AddChart(new Chart(reportsPath, compressionPlot, energyPlot, errorPlot) { Columns = 3 })
                .AddSection("Optimal Rank Selection")
                .AddParagraph("The optimal rank depends on the application:")
                .AddList(new[] {
                    "Maximum compression: Choose smallest r with acceptable error",
                    "Maximum fidelity: Choose r that captures >95% energy",
                    "Balanced approach: Find knee in error vs rank curve"
                })
                .AddSection("Simulation Parameters")
                .AddTable(new[,] {
                    { "Matrix dimensions", "7168 × 2048" },
                    { "Original size", "58.7 MB (float32)" },
                    { "Rank range tested", "4 to 256" },
                    { "Target compression", "6:1 (→ 9.8 MB)" }
                })
                .Build();

            Assert.Pass("Compression analysis report generated");
        }

        [Test]
        public void GenerateScientificGalleryReport()
        {
            var luminy = new LuminyBuilder();
            var displays = new List<Display>();

            // 1. Line plot (sine wave)
            var x = Enumerable.Range(0, 100).Select(i => i * 0.1f).ToArray();
            var y = x.Select(v => MathF.Sin(v)).ToArray();
            displays.Add(luminy.Display(luminy.Line(x, y, color: new Color(0.2f, 0.4f, 0.8f))) with
            {
                Title = "Sine Wave",
                XLabel = "x",
                YLabel = "sin(x)",
                AxesStyle = AxesStyle.Normal
            });

            // 2. Point plot (scatter)
            var scatterX = Enumerable.Range(0, 50).Select(i => Random.Shared.NextSingle() * 10).ToArray();
            var scatterY = scatterX.Select(v => v + Random.Shared.NextSingle() * 2 - 1).ToArray();
            displays.Add(luminy.Display(luminy.Points(scatterX, scatterY, color: new Color(0.8f, 0.2f, 0.2f))) with
            {
                Title = "Scatter Plot",
                XLabel = "X values",
                YLabel = "Y values"
            });

            // 3. Bar chart
            var barValues = new[] { 4.5f, 3.2f, 6.7f, 2.1f, 5.5f };
            displays.Add(luminy.Display(luminy.Bars(barValues, color: new Color(0.2f, 0.7f, 0.3f))) with
            {
                Title = "Bar Chart",
                XLabel = "Category",
                YLabel = "Value"
            });

            // 4. Histogram
            var normalData = GenerateNormalData(1000)
                .Where(x => -5 <= x && x <= 5)
                .ToArray();
            displays.Add(luminy.Display(luminy.Histogram(normalData, color: new Color(0.7f, 0.3f, 0.8f))) with
            {
                Title = "Normal Distribution",
                XLabel = "Value",
                YLabel = "Frequency"
            });

            // 5. Box plot (Multi-series)
            var boxData1 = Enumerable.Range(0, 100).Select(_ => Random.Shared.NextSingle() * 10).ToArray();
            var boxData2 = Enumerable.Range(0, 100).Select(_ => 3f + Random.Shared.NextSingle() * 4).ToArray();
            var boxData3 = Enumerable.Range(0, 100).Select(_ => 5f + Random.Shared.NextSingle() * 2).ToArray();
            
            displays.Add(luminy.Display(
                    luminy.Box(boxData1, 1, color: new Color(0.2f, 0.4f, 0.8f)),
                    luminy.Box(boxData2, 2, color: new Color(0.8f, 0.4f, 0.2f)),
                    luminy.Box(boxData3, 3, color: new Color(0.2f, 0.8f, 0.4f))
                ) with
                {
                    Title = "Multiple Box Plots",
                    XLabel = "Group",
                    YLabel = "Value",
                    XRange = new Luminy.Model.Range(0.5f, 3.5f)
                });

            // 6. Heatmap
            var heatmapData = new float[20, 30];
            for (var i = 0; i < 20; i++)
            for (var j = 0; j < 30; j++)
                heatmapData[i, j] = MathF.Sin(i * 0.3f) * MathF.Cos(j * 0.2f);

            displays.Add(luminy.Display(luminy.Matrix(heatmapData, label: "Heatmap")) with
            {
                Title = "Heatmap",
                XLabel = "X coordinate",
                YLabel = "Y coordinate"
            });

            // Create gallery report
            var report = new ReportBuilder(reportsPath, "plot_gallery.html")
                .AddTitle("Luminy Plotting Library Gallery")
                .AddSection("Overview")
                .AddParagraph("This gallery demonstrates all plot types available in the Luminy library. Each plot is generated from synthetic data.")
                .AddSection("Plot Gallery")
                .AddChart(new Chart(reportsPath, displays.ToArray()) { Columns = 2 })
                .AddSection("Library Features")
                .AddList(new[] {
                    "SVG-based rendering (crisp at any resolution)",
                    "Multiple plot types (line, point, bar, histogram, heatmap, etc.)",
                    "Logarithmic and linear scales",
                    "Customizable colors and styles",
                    "Responsive HTML output"
                })
                .Build();

            Assert.Pass("Scientific gallery generated");
        }

        private static float[] GenerateSingularValues(int count, float decayRate)
        {
            var values = new float[count];
            for (var i = 0; i < count; i++)
            {
                // Exponential decay with slight noise
                values[i] = MathF.Exp(-decayRate * i) * (1f + 0.1f * Random.Shared.NextSingle())+ 0.001f + (0.001f*(count - i))/count;
            }
            return values;
        }

        private static Display CreateSingularValueDisplay(float[] singularValues)
        {
            var x = Enumerable.Range(1, singularValues.Length).Select(i => (float)i).ToArray();

            var luminy = new LuminyBuilder { Width = 800, Height = 500 };

            return luminy.Display(
                luminy.Line(x, singularValues, color: new Color(0.2f, 0.4f, 0.8f)),
                luminy.Points(x, singularValues, color: new Color(0.8f, 0.2f, 0.2f))
            ) with {
                Title = "Singular Value Decay",
                XLabel = "Rank (i)",
                YLabel = "Singular Value (σᵢ)",
                XScaleType = ScaleType.Linear,
                YScaleType = ScaleType.Logarithmic
            };
        }

        public static float[] GenerateNormalData(int count, float mean = 0, float stdDev = 1)
        {
            var data = new float[count];
            for (var i = 0; i < count; i += 2)
            {
                // Box-Muller transform
                var u1 = Random.Shared.NextSingle();
                var u2 = Random.Shared.NextSingle();
                var z0 = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Cos(2.0f * MathF.PI * u2);
                var z1 = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2);

                data[i] = mean + stdDev * z0;
                if (i + 1 < count) data[i + 1] = mean + stdDev * z1;
            }
            return data;
        }
    }
}