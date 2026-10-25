# LinePlot

`LinePlot` renders continuous 2D curves by connecting ordered coordinate points $(x_i, y_i)$ with vector paths. It is the core primitive for time series, mathematical functions, physical oscillations, and scientific measurements.

---

## Visual Preview

![LinePlot Harmonic Signal Analysis](./line_plot.svg)

---

## Code Examples

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, LinePlot } from 'luminy-react';

export const HarmonicSignalChart: React.FC = () => {
  const samples = 120;
  const step = (4.0 * Math.PI) / samples;
  const x = Array.from({ length: samples }, (_, i) => i * step);
  const pureSin = x.map(t => Math.sin(2.0 * t));
  const dampedCos = x.map(t => Math.cos(2.0 * t) * Math.exp(-0.15 * t));

  return (
    <Display
      width={700}
      height={420}
      title="Harmonic Signal Analysis"
      xLabel="Time t (s)"
      yLabel="Signal Amplitude"
      showGrid={true}
      showLegend={true}
      axesStyle="Frame"
    >
      <LinePlot
        x={x}
        y={pureSin}
        label="Harmonic Sine"
        color="#0284c7"
        strokeWidth={2}
      />
      <LinePlot
        x={x}
        y={dampedCos}
        label="Damped Cosine"
        color="#f97316"
        strokeWidth={2}
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

var luminy = new LuminyBuilder { Width = 700, Height = 420 };

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

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<luminy:Display
    PlotTitle="Harmonic Signal Analysis"
    XAxisLabel="Time t (s)"
    YAxisLabel="Signal Amplitude"
    ShowGrid="True"
    ShowLegend="True"
    Width="700"
    Height="420">
    <luminy:LinePlot
        XValues="{Binding TimeData}"
        YValues="{Binding SineData}"
        Stroke="#0284c7"
        StrokeThickness="2"
        Label="Harmonic Sine" />
    <luminy:LinePlot
        XValues="{Binding TimeData}"
        YValues="{Binding DampedCosineData}"
        Stroke="#f97316"
        StrokeThickness="2"
        Label="Damped Cosine" />
</luminy:Display>
```

---

## Parameters & Props Reference

| Parameter / Prop | React (`LinePlotProps`) | C# (`LinePlot`) | WPF (`LinePlot`) | Description |
| :--- | :--- | :--- | :--- | :--- |
| **X Data** | `x: number[]` | `float[] X` | `XValues` (`IEnumerable<double>`) | Abscissa coordinate points |
| **Y Data** | `y: number[]` | `float[] Y` | `YValues` (`IEnumerable<double>`) | Ordinate coordinate points |
| **Label** | `label?: string` | `string Label` | `Label` (`string`) | Legend label identifier |
| **Stroke Color** | `color?: string` | `Color Color` | `Stroke` (`Brush`) | Path stroke color |
| **Stroke Width** | `strokeWidth?: number` | `float StrokeWidth` | `StrokeThickness` (`double`) | Thickness of the path curve |
| **Dash Pattern** | `strokeDasharray?: string` | — | `StrokeDashArray` (`DoubleCollection`) | Dashed or dotted line pattern |
