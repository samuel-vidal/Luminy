# Getting Started

Luminy is a cross-platform scientific data visualization suite designed from the ground up for high-precision, zero-dependency mathematical plotting.

Whether you are targeting **.NET console/server applications**, **Windows desktop with WPF**, or **modern web applications with React**, Luminy provides an identical mental model and component hierarchy.

---

## The Three Flavors

| Platform | Package | Technology | Use Case |
| :--- | :--- | :--- | :--- |
| **.NET / C#** | `Luminy` | Pure C# SVG Builder | Server-side rendering, export, CLI tools, reports |
| **WPF / Desktop** | `Luminy.Wpf` | DirectX `StreamGeometry` | High-frequency Windows desktop apps, MVVM data binding |
| **Web / React** | `luminy-react` | React Virtual DOM & SVG | Interactive dashboards, web apps, zero-runtime overhead |

---

## Quickstart Comparison

Here is how you render a basic harmonic sine signal across all three ecosystems:

### ⚛️ React (TSX)

```tsx
import React from 'react';
import { Display, LinePlot } from 'luminy-react';

export const MyChart: React.FC = () => {
  const x = [0, 1, 2, 3, 4, 5, 6];
  const y = [0, 0.84, 0.91, 0.14, -0.76, -0.96, -0.28];

  return (
    <Display width={600} height={400} title="Sine Wave" showGrid={true}>
      <LinePlot x={x} y={y} label="Sine" color="#0284c7" strokeWidth={2} />
    </Display>
  );
};
```

### 🔷 C# (.NET Core)

```csharp
using Luminy.Builders;
using Luminy.Model;

float[] x = [0, 1, 2, 3, 4, 5, 6];
float[] y = [0, 0.84f, 0.91f, 0.14f, -0.76f, -0.96f, -0.28f];

var display = LuminyBuilder.Chart()
    .WithDimensions(600, 400)
    .WithTitle("Sine Wave")
    .WithGrid()
    .Add(new LinePlot(x, y, "Sine", Color.FromHex("#0284c7"), StrokeWidth: 2))
    .Build();

string svg = display.ToSvg();
```

### 🪟 WPF (XAML / C#)

```xaml
<Window xmlns:luminy="clr-namespace:Luminy.Wpf;assembly=Luminy.Wpf">
    <luminy:Display
        PlotTitle="Sine Wave"
        ShowGrid="True"
        Width="600"
        Height="400">
        <luminy:LinePlot
            XValues="{Binding TimePoints}"
            YValues="{Binding Amplitudes}"
            Stroke="DodgerBlue"
            StrokeThickness="2" />
    </luminy:Display>
</Window>
```

---

## Core Principles

1. **Automatic Global Range Detection**:
   The `<Display>` container automatically inspects all child plots synchronously to compute the global bounding box $[X_{\min}, X_{\max}] \times [Y_{\min}, Y_{\max}]$, guaranteeing perfect scale synchronization across multiple overlapping series.

2. **Decoupled Coordinate Mapping**:
   Mathematical data coordinates $(u, v)$ are mapped to physical viewport coordinates $(x, y)$ via an affine transformation `CoordinateMap`. Plots only draw inside their logical space.

3. **Zero Runtime Bloat**:
   No third-party canvas or WebGL runtimes are required. Everything is lightweight vector geometry.
