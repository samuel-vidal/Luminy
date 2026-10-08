using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminy.Model;
using Luminy.Builders;

namespace Luminy.Tests
{
    public class FourierSquareWaveReport
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
        public void GenerateReport()
        {
            var luminy = new LuminyBuilder { Width = 800, Height = 500 };

            // Define square wave function over [0,1], period = 1
            var x = Enumerable.Range(0, 200).Select(i => i * 0.005f).ToArray(); // 200 points

            var squareWave = x.Select(t =>
                (t % 1.0f) < 0.5f ? 1.0f : -1.0f
            ).ToArray();

            var plots = new List<Plot>();

            // Add original square wave (dashed or different style?)
            plots.Add(luminy.Line(x, squareWave, "Square wave (original)"));

            // Add Fourier approximations from 1 to 10 harmonics
            for (var n = 1; n <= 10; n++)
            {
                var fourierApprox = x.Select(t =>
                    FourierSeries(t, n)
                ).ToArray();

                plots.Add(luminy.Line(x, fourierApprox, $"Fourier n={n}"));
            }

            // Create display — but wait, how do I add a legend? Not in skill.md.
            var display = luminy.Display(plots.ToArray()) with
            {
                Title = "Fourier Decomposition of Square Wave (Period 1)",
                XLabel = "Time (t)",
                YLabel = "Amplitude",
                XScaleType = ScaleType.Linear,
                YScaleType = ScaleType.Linear,
                ShowGrid = true
            };

            new ReportBuilder(reportsPath, "fourier_report.html")
                .AddTitle("Fourier Analysis")
                .AddParagraph("Square wave approximated by 1 to 10 harmonics.")
                .AddChart(new Chart(reportsPath, display) { Columns = 1 })
                .Build();
        }

        private float FourierSeries(float t, int harmonics)
        {
            // Square wave Fourier series for period 1, amplitude 1
            float sum = 0;
            for (var k = 1; k <= harmonics; k++)
            {
                var n = 2 * k - 1; // odd harmonics only
                sum += (4.0f / (float)Math.PI) * (1.0f / n) * (float)Math.Sin(2 * Math.PI * n * t);
            }

            return sum;
        }
    }
}