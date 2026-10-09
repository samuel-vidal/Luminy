# Luminy 🌟

**Elegant SVG math plotting and scientific HTML reports for .NET — zero external dependencies.**

[![NuGet](https://img.shields.io/nuget/v/Luminy.svg)](https://www.nuget.org/packages/Luminy)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

> **Pure Vector Scientific Visualization** — Generate crisp SVG charts and complete HTML research reports with modern, idiomatic C#.

Luminy is a lightweight, standalone .NET library designed for generating high-quality scientific plots, data visualizations, and automated research reports. Built from the ground up for simplicity, performance, and clean ergonomics, Luminy produces vector graphics that remain perfectly sharp at any zoom level, without relying on bulky external graphic packages or native wrapper runtimes.

---

## 🚀 Why Luminy?

- **📦 Zero Dependencies**: Pure managed C# (.NET 9). No SkiaSharp, no System.Drawing, no C++ native bridges. Just reference the package and run anywhere.
- **🖼️ Pure Vector SVGs**: Output clean, responsive SVG markup ready for embedding into HTML dashboards, web applications, or scientific papers.
- **📈 Scientific First**: Built-in linear and logarithmic scales, "nice" tick calculation, multi-series auto-coloring, and statistical summaries.
- **✨ Idiomatic C#**: Immutable records, non-destructive mutation (`with`), primary constructors, and fluent document builders.
- **📑 Integrated HTML Reports**: Directly generate publication-ready HTML documents with structured headings, paragraphs, formatted tables, code blocks, and responsive chart grids.
- **⚡ High-Performance Tensor Imaging**: Zero-dependency 8-bit indexed PNG encoder (LUT256) and unmanaged cache-aligned buffers (`BitmapBuffer`) for ultra-compact AI heatmaps.
- **🐧 100% Cross-Platform**: Runs natively on Linux, macOS, and Windows with zero configuration.

---

## 🧠 AI Friendly!

Check our short and sweet skill files designed for LLMs, coding agents, and quick developer reference:
- [**Plotting & Scientific Reports Skill**](skill.md) — Vector SVG charts, composable displays, and HTML research documents.
- [**Imaging & Tensor Heatmaps Skill**](imaging_skill.md) — Unmanaged aligned buffers (`BitmapBuffer<TPixel>`) and high-performance 8-bit indexed PNG encoding (LUT256).

---

## 💡 Quick Start

### 1. Create a Simple SVG Plot

```csharp
using Luminy.Model;
using Luminy.Builders;

var luminy = new LuminyBuilder { Width = 700, Height = 450, ShowGrid = true };

float[] x = [1f, 2f, 3f, 4f, 5f];
float[] y = [10f, 25f, 18f, 32f, 28f];

var display = luminy.Display(
    luminy.Line(x, y, label: "Sample Trend", color: new Color(0.2f, 0.5f, 0.9f), strokeWidth: 2)
) with {
    Title = "Sample Data Trend",
    XLabel = "Time (s)",
    YLabel = "Value (Units)",
    ShowGrid = true
};

// Generate pure SVG string
string svg = display.ToSvg();
```

---

### 2. Generate a Complete HTML Scientific Report

```csharp
using Luminy.Model;
using Luminy.Builders;

var luminy = new LuminyBuilder { Width = 800, Height = 500, ShowGrid = true };

// Generate data
float[] x = Enumerable.Range(0, 100).Select(i => i * 0.1f).ToArray();
float[] sinY = x.Select(v => MathF.Sin(v)).ToArray();
float[] cosY = x.Select(v => MathF.Cos(v)).ToArray();

// Compose multi-series visualization
var display = luminy.Display(
    luminy.Line(x, sinY, label: "Sin(x)"),
    luminy.Line(x, cosY, label: "Cos(x)")
) with {
    Title = "Trigonometric Signals",
    XLabel = "Time (s)",
    YLabel = "Amplitude",
    LegendPosition = LegendPosition.TopRight
};

// Build the HTML report with embedded charts
string outputDir = "./reports";

new ReportBuilder(outputDir, "experiment_report.html")
    .AddTitle("Harmonic Signal Analysis")
    .AddSection("Visual Spectrum")
    .AddParagraph("The following chart illustrates the phase relationship between sine and cosine signals.")
    .AddChart(display, "Figure 1: Trigonometric Comparison")
    .AddSection("Key Metrics")
    .AddTable(new[,] {
        { "Metric", "Value" },
        { "Samples", x.Length.ToString() },
        { "Phase Offset", "π / 2" }
    })
    .Build();
```

---

## 📊 Supported Visualizations

| Plot Type | Description | Builder Method |
| :--- | :--- | :--- |
| **LinePlot** | Classic continuous curves, signals, and multi-harmonic plots | `luminy.Line(x, y, label)` |
| **PointPlot** | Scatter plots with automatic filtering of non-finite values (NaN / Inf) | `luminy.Points(x, y, label)` |
| **BarChart** | Discrete category comparisons with automatic spacing calculation | `luminy.Bars(heights, label)` |
| **HistogramPlot** | Frequency distribution analysis with custom binning | `luminy.Histogram(data, binCount, label)` |
| **MatrixPlot** | 2D color heatmaps with smooth gradient interpolation | `luminy.Matrix(data2D, label)` |
| **BoxPlot** | Tukey-style statistical box & whiskers with automatic outlier detection | `luminy.Box(data, x, label)` |

---

## 🎯 Advanced Features

### Logarithmic & Linear Scales
Easily configure logarithmic scales independently on either axis:

```csharp
var display = luminy.Display(spectrumPlot) with {
    XScaleType = ScaleType.Logarithmic,
    YScaleType = ScaleType.Linear
};
```

### Axes Styles
Select the framing mode that fits your context:
- `AxesStyle.Frame` (default): Boxed frame ideal for scientific papers and dashboards.
- `AxesStyle.Normal`: Traditional Cartesian axes crossing at $(0,0)$.
- `AxesStyle.Boxed`: Frame with tick marks distributed on all bounding sides.
- `AxesStyle.None`: Frameless display for sparklines and minimal badges.

### Responsive Multi-Chart Grids
Embed several charts side-by-side in HTML reports with responsive flex wrapping:

```csharp
new ReportBuilder(outputDir, "dashboard.html")
    .AddChart(new Chart(outputDir, plotA, plotB, plotC) { Columns = 3 })
    .Build();
```

---

## 🛠 Installation

### .NET CLI
```bash
dotnet add package Luminy
```

### Package Manager
```bash
Install-Package Luminy
```

### Project Reference
```xml
<PackageReference Include="Luminy" Version="1.0.0" />
```

---

## 🔧 Requirements

- **.NET 9.0** SDK or later
- Cross-platform: **Linux**, **Windows**, **macOS**

---

## 🤝 Collaboration

This project was developed through human-AI collaboration:

- **Project Lead & Architect**: Samuel Alexandre Vidal
- **AI Collaborators**:
  - DeepSeek V3.1
  - Gemini 3.5
  - Gemini 3.8
  - GLM 5

The development process featured continuous collaboration between human vision and multiple cutting-edge AI systems, each contributing unique strengths to create a production-quality library.

---

## 📄 License

MIT License - see [LICENSE.txt](LICENSE.txt) for details.

---

## 🚀 Getting Involved

- **Found a bug?** [Open an issue](https://github.com/samuel-vidal/Luminy/issues)
- **Have an idea or feedback?** [Start a discussion](https://github.com/samuel-vidal/Luminy/discussions)
- **Want to contribute?** Pull requests are welcome!
