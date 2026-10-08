# AI Skill: Luminy Plotting Library

Luminy is an elegant C# library for generating SVG plots and HTML scientific reports. 
**Installation**: Add a project reference to `Luminy` (or `dotnet add package Luminy` via NuGet).

## The Mental Model

Luminy's architecture follows a strict, composable hierarchy:
- **Report**: The top-level HTML document. Contains titles, sections, paragraphs, tables, code blocks, and `Chart` objects.
- **Chart (Figure)**: A layout container holding one or more `Display` objects. It is instantiated with an output directory where it saves its SVGs.
- **Display**: A shared coordinate space (axes, grids, scales). It is constructed via `LuminyBuilder` and holds one or more `Plot` objects.
- **Plot**: The actual data visualization (`LinePlot`, `PointPlot`, `BarChart`, `HistogramPlot`, `MatrixPlot`, `BoxPlot`). Multiple plots can coexist inside the same Display.

## Core Data Types & Namespaces
Always include these namespaces:
```csharp
using Luminy.Model;    // contains Plots, Displays, and Color
using Luminy.Builders; // contains LuminyBuilder and ReportBuilder
```
**Convention**: Luminy uses `float[]` (and `float[,]` for heatmaps) for mathematical data.

## API Signatures & Collections
You use the `LuminyBuilder` to construct plots and displays:
- `luminy.Line(float[] x, float[] y, string label = "", Color? color = null, int strokeWidth = 2)`: Generates a line plot.
- `luminy.Points(float[] x, float[] y, string label = "", Color? color = null, int pointSize = 3)`: Generates a scatter plot (filters non-finite values).
- `luminy.Bars(float[] heights, string label = "", Color? color = null, float? barWidth = null)`: Generates vertical bar charts.
- `luminy.Histogram(float[] data, int binCount = 10, string label = "", Color? color = null)`: Generates a histogram distribution.
- `luminy.Matrix(float[,] data, string label = "", Color? colorMin = null, Color? colorMax = null)`: Generates a 2D heatmap.
- `luminy.Box(float[] data, float x = 0, string label = "", Color? color = null, float width = 0.8f)`: Generates a Tukey box plot.
- `luminy.Display(params Plot[] plots)`: Constructs an immutable display. If you dynamically generate plots in a list, use `luminy.Display(plots.ToArray())`.
- `new ReportBuilder(outputDir, fileName).Build()`: Generates the HTML report and writes it to disk.

## Full Minimal Working Example

This complete example demonstrates the workflow from data generation to saving the final HTML report to disk.

```csharp
using System;
using System.Linq;
using System.Collections.Generic;
using Luminy.Model;
using Luminy.Builders;

public class LuminyExample
{
    public void GenerateReport()
    {
        // 1. Instantiate the Builder
        var luminy = new LuminyBuilder { Width = 800, Height = 500 };

        // 2. Generate Data (Must be float!)
        float[] xData = Enumerable.Range(0, 100).Select(i => i * 0.1f).ToArray();
        
        // 3. Create Plots
        var plots = new List<Plot>();
        plots.Add(luminy.Line(xData, xData.Select(x => (float)Math.Sin(x)).ToArray(), "Sine"));
        plots.Add(luminy.Line(xData, xData.Select(x => (float)Math.Cos(x)).ToArray(), "Cosine"));

        // 4. Compose the Display
        var display = luminy.Display(plots.ToArray()) with {
            Title = "Trigonometric Functions",
            XLabel = "Time (s)",
            YLabel = "Amplitude",
            XScaleType = ScaleType.Linear,
            YScaleType = ScaleType.Linear,
            ShowGrid = true
        };

        // 5. Generate HTML Report and SVGs
        string outputDir = "./output";
        
        new ReportBuilder(outputDir, "math_report.html")
            .AddTitle("Mathematical Analysis")
            .AddChart(display)
            .Build(); // Writes the HTML and SVG files to disk immediately
    }
}
```
