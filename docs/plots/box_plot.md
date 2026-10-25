# BoxPlot (Tukey Box & Whisker Plots)

`BoxPlot` provides a compact five-number statistical summary of continuous data distributions using the standard **Tukey box-and-whisker** representation.

---

## Visual Concept & Anatomy

```
         Outlier ●
                 │
              ┌──┴──┐  Upper Whisker Cap
                 │     Upper Whisker (<= Q3 + 1.5*IQR)
              ┌──┴──┐
              │ Q3  │  75th Percentile
              ├───┬─┤  Median (50th Percentile)
              │ Q1  │  25th Percentile
              └──┬──┘
                 │     Lower Whisker (>= Q1 - 1.5*IQR)
              └──┬──┘  Lower Whisker Cap
                 │
         Outlier ●
```

---

## Code Examples

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, BoxPlot } from 'luminy-react';

export const ClinicalTrialBoxPlot: React.FC = () => {
  const control = [10.2, 10.8, 11.1, 9.7, 10.5, 11.4, 10.3, 14.2]; // 14.2 is outlier
  const treated = [12.5, 13.1, 12.8, 13.9, 12.4, 13.2, 12.7];

  return (
    <Display
      width={700}
      height={420}
      title="Clinical Biomarker Response"
      xLabel="Cohort (1: Control, 2: Treated)"
      yLabel="Biomarker Level (mg/dL)"
      showGrid={true}
      showLegend={true}
    >
      <BoxPlot
        data={control}
        x={1}
        width={0.65}
        color="#0284c7"
        label="Control Cohort"
      />
      <BoxPlot
        data={treated}
        x={2}
        width={0.65}
        color="#10b981"
        label="Treated Cohort"
      />
    </Display>
  );
};
```

### 🔷 C# (.NET Core)

```csharp
using Luminy.Builders;
using Luminy.Model;

float[] control = [10.2f, 10.8f, 11.1f, 9.7f, 10.5f, 11.4f, 10.3f, 14.2f];

var display = LuminyBuilder.Chart()
    .WithDimensions(700, 420)
    .WithTitle("Clinical Biomarker Response")
    .WithGrid()
    .Add(new BoxPlot(control, X: 1f, Label: "Control Cohort", Color: Color.FromHex("#0284c7"), Width: 0.65f))
    .Build();

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<luminy:Display
    PlotTitle="Clinical Biomarker Response"
    XAxisLabel="Cohort"
    YAxisLabel="Biomarker Level"
    ShowGrid="True"
    Width="700"
    Height="420">
    <luminy:BoxPlot
        Data="{Binding ControlData}"
        XPosition="1"
        BoxWidth="0.65"
        Stroke="#0284c7"
        Label="Control Cohort" />
</luminy:Display>
```

---

## Parameters & Props Reference

| Parameter / Prop | React (`BoxPlotProps`) | C# (`BoxPlot`) | WPF (`BoxPlot`) | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Data Points** | `data: number[]` | `float[] Data` | `Data` (`IEnumerable<double>`) | Sample data points to summarize |
| **X Center** | `x?: number` | `float X = 1f` | `XPosition` (`double`) | Horizontal position of the box |
| **Box Width** | `width?: number` | `float Width = 0.8f` | `BoxWidth` (`double`) | Box width in coordinate units |
| **Stroke Color** | `color?: string` | `Color Color` | `Stroke` (`Brush`) | Color of box, median, and whiskers |
| **Fill Color** | `fill?: string` | — | `Fill` (`Brush`) | Interior fill of the quartile box |
| **Outlier Size** | `outlierRadius?: number` | — | `OutlierRadius` (`double`) | Radius of individual outlier circles |
| **Label** | `label?: string` | `string Label` | `Label` (`string`) | Legend series identifier |
