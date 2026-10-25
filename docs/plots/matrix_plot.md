# MatrixPlot (2D Heatmaps)

`MatrixPlot` renders a 2D numerical array ($M \times N$) as a color-coded heatmap. Cell values are dynamically normalized to the range $[0.0, 1.0]$ and interpolated between configurable minimum and maximum colors.

---

## Visual Preview

![MatrixPlot 2D Intensity Ripple Field](./matrix_plot.svg)

---

## Code Examples

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, MatrixPlot } from 'luminy-react';

export const HeatmapDemo: React.FC = () => {
  const size = 12;
  const center = (size - 1) / 2.0;
  const matrix: number[][] = [];

  for (let i = 0; i < size; i++) {
    const row: number[] = [];
    for (let j = 0; j < size; j++) {
      const di = (i - center) / center;
      const dj = (j - center) / center;
      const dist = Math.sqrt(di * di + dj * dj);
      row.push(Math.cos(dist * 2.5 * Math.PI) * Math.exp(-dist * 0.7));
    }
    matrix.push(row);
  }

  return (
    <Display
      width={700}
      height={460}
      title="2D Intensity Ripple Field"
      xLabel="Matrix Column (j)"
      yLabel="Matrix Row (i)"
      showGrid={false}
      showLegend={false}
      axesStyle="Frame"
    >
      <MatrixPlot
        data={matrix}
        colorMin="#f0f9ff"
        colorMax="#0284c7"
        cellStroke="rgba(255, 255, 255, 0.4)"
        cellStrokeWidth={0.5}
        label="Thermal Intensity"
      />
    </Display>
  );
};
```

### 🔷 C# (.NET Core)

```csharp
using System;
using Luminy.Builders;
using Luminy.Model;

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

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<luminy:Display
    PlotTitle="2D Intensity Ripple Field"
    XAxisLabel="Matrix Column (j)"
    YAxisLabel="Matrix Row (i)"
    Width="700"
    Height="460">
    <luminy:MatrixPlot
        Data2D="{Binding RippleMatrix}"
        ColorMin="#f0f9ff"
        ColorMax="#0284c7"
        CellStroke="White"
        CellStrokeThickness="0.5"
        Label="Thermal Intensity" />
</luminy:Display>
```

---

## Parameters & Props Reference

| Parameter / Prop | React (`MatrixPlotProps`) | C# (`MatrixPlot`) | WPF (`MatrixPlot`) | Description |
| :--- | :--- | :--- | :--- | :--- |
| **2D Data** | `data: number[][]` | `float[,] Data` | `Data2D` (`double[,]`) | 2D matrix array |
| **Minimum Color** | `colorMin?: string` | `Color ColorMin` | `ColorMin` (`Color`) | Color mapped to the minimum value |
| **Maximum Color** | `colorMax?: string` | `Color ColorMax` | `ColorMax` (`Color`) | Color mapped to the maximum value |
| **Cell Border** | `cellStroke?: string` | — | `CellStroke` (`Brush`) | Border line color separating cells |
| **Border Width** | `cellStrokeWidth?: number` | — | `CellStrokeThickness` (`double`) | Cell border stroke thickness |
| **Label** | `label?: string` | `string Label` | `Label` (`string`) | Legend series identifier |
