# BarChart (Column & Bar Charts)

`BarChart` renders categorical and discrete numeric distributions as vertical columns. Bars project outward from the zero baseline ($Y = 0$), supporting both **positive earnings** and **negative deficits**.

---

## Visual Concept

```
  Y ▲
    │       ┌──┐
    │       │  │
    │  ┌──┐ │  │
  0 ┼──┼──┼─┼──┼───┼──┼──► X
    │  │  │        │  │
    │              └──┘ (Negative)
```

---

## Code Examples

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, BarChart } from 'luminy-react';

export const FinancialBarChart: React.FC = () => {
  const quarters = [1, 2, 3, 4];
  const netEarnings = [45, 62, -18, 74];

  return (
    <Display
      width={700}
      height={420}
      title="Quarterly Fiscal Results"
      xLabel="Quarter (Q1 - Q4)"
      yLabel="Net Earnings (k€)"
      showGrid={true}
      showLegend={true}
    >
      <BarChart
        x={quarters}
        heights={netEarnings}
        barWidth={0.65}
        color="#0d9488"
        label="Net Results (k€)"
      />
    </Display>
  );
};
```

### 🔷 C# (.NET Core)

```csharp
using Luminy.Builders;
using Luminy.Model;

float[] quarters = [1f, 2f, 3f, 4f];
float[] earnings = [45f, 62f, -18f, 74f];

var display = LuminyBuilder.Chart()
    .WithDimensions(700, 420)
    .WithTitle("Quarterly Fiscal Results")
    .WithGrid()
    .Add(new BarChart(earnings, quarters, BarWidth: 0.65f, Label: "Net Results (k€)", Color: Color.FromHex("#0d9488")))
    .Build();

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<luminy:Display
    PlotTitle="Quarterly Fiscal Results"
    XAxisLabel="Quarter"
    YAxisLabel="Net Earnings (k€)"
    ShowGrid="True"
    Width="700"
    Height="420">
    <luminy:BarChart
        XValues="{Binding Quarters}"
        Heights="{Binding NetEarnings}"
        BarWidth="0.65"
        Fill="#0d9488"
        Label="Net Results" />
</luminy:Display>
```

---

## Parameters & Props Reference

| Parameter / Prop | React (`BarChartProps`) | C# (`BarChart`) | WPF (`BarChart`) | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Heights** | `heights: number[]` | `float[] Heights` | `Heights` (`IEnumerable<double>`) | Bar magnitudes relative to $Y=0$ |
| **X Centers** | `x?: number[]` | `float[]? X` | `XValues` (`IEnumerable<double>`) | Central coordinate for each bar |
| **Bar Width** | `barWidth?: number` | `float? BarWidth` | `BarWidth` (`double`) | Width of each bar in data units |
| **Bar Color** | `color?: string` | `Color Color` | `Fill` (`Brush`) | Color fill of the bars |
| **Label** | `label?: string` | `string Label` | `Label` (`string`) | Legend series identifier |
