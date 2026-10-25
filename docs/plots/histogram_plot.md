# HistogramPlot (Frequency Distributions)

`HistogramPlot` computes and renders the frequency distribution of continuous numerical data. It automatically discretizes sample points into $k$ contiguous bins and draws proportional frequency rectangles.

---

## Visual Concept

```
  Count ▲
        │         ┌──┐
        │      ┌──┤  ├──┐
        │   ┌──┤  │  │  ├──┐
        │   │  │  │  │  │  │
        └───┴──┴──┴──┴──┴──┴──► Value (x)
             k = 5 Bins
```

---

## Code Examples

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, HistogramPlot } from 'luminy-react';

export const SampleHistogram: React.FC = () => {
  // Continuous measurement data
  const sensorReadings = [10.2, 11.5, 9.8, 10.4, 12.1, 10.8, 11.0, 9.5, 10.7, 11.2, 10.5];

  return (
    <Display
      width={700}
      height={420}
      title="Sensor Noise Distribution"
      xLabel="Voltage (V)"
      yLabel="Frequency Count"
      showGrid={true}
      showLegend={true}
    >
      <HistogramPlot
        data={sensorReadings}
        binCount={15}
        color="#8b5cf6"
        stroke="rgba(255, 255, 255, 0.3)"
        label="Sensor Readings"
      />
    </Display>
  );
};
```

### 🔷 C# (.NET Core)

```csharp
using Luminy.Builders;
using Luminy.Model;

float[] sensorReadings = [10.2f, 11.5f, 9.8f, 10.4f, 12.1f, 10.8f, 11.0f, 9.5f, 10.7f];

var display = LuminyBuilder.Chart()
    .WithDimensions(700, 420)
    .WithTitle("Sensor Noise Distribution")
    .WithGrid()
    .Add(new HistogramPlot(sensorReadings, BinCount: 15, Label: "Sensor Readings", Color: Color.FromHex("#8b5cf6")))
    .Build();

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<luminy:Display
    PlotTitle="Sensor Noise Distribution"
    XAxisLabel="Voltage (V)"
    YAxisLabel="Frequency Count"
    ShowGrid="True"
    Width="700"
    Height="420">
    <luminy:HistogramPlot
        Data="{Binding SensorReadings}"
        BinCount="15"
        Fill="#8b5cf6"
        Label="Sensor Readings" />
</luminy:Display>
```

---

## Parameters & Props Reference

| Parameter / Prop | React (`HistogramPlotProps`) | C# (`HistogramPlot`) | WPF (`HistogramPlot`) | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Data Array** | `data: number[]` | `float[] Data` | `Data` (`IEnumerable<double>`) | Continuous raw numerical values |
| **Bin Count** | `binCount?: number` | `int BinCount` | `BinCount` (`int`) | Number of equal-width intervals |
| **Fill Color** | `color?: string` | `Color Color` | `Fill` (`Brush`) | Bar fill color |
| **Bin Stroke** | `stroke?: string` | — | `Stroke` (`Brush`) | Color of bin separation borders |
| **Border Width** | `strokeWidth?: number` | — | `StrokeThickness` (`double`) | Border thickness between bins |
| **Label** | `label?: string` | `string Label` | `Label` (`string`) | Legend series identifier |
