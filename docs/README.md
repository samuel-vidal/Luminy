# Luminy Documentation

Welcome to the **Luminy** documentation. Luminy is a zero-dependency, pure vector mathematical plotting ecosystem providing identical declarative APIs and retained-mode architectures across **.NET Core**, **WPF**, and **React**.

---

## 📚 Ecosystem Guides

- **[Getting Started Guide](./guide/getting-started.md)**: Installation, quickstart examples for .NET, WPF, and React, and core principles.
- **[Architecture & Design](./guide/architecture.md)**: The retained-mode paradigm, synchronous single-pass child inspection, and 100% DRY algorithm sharing.

---

## 🎨 Plot Types Catalog

Luminy provides 7 foundational scientific plot types, fully synchronized across platforms:

| Plot Type | Category | Primary Cas d'Usage | Supported Platforms |
| :--- | :--- | :--- | :--- |
| **[LinePlot](./plots/line_plot.md)** | Continuous | Waveforms, mathematical functions, time series | .NET, WPF, React |
| **[DonutPlot](./plots/donut_plot.md)** | Radial | Proportions, market shares, pie & donut charts | .NET, WPF, React |
| **[PointPlot](./plots/point_plot.md)** | Scatter | Coordinate data, clustering, bivariate correlations | .NET, WPF, React |
| **[BarChart](./plots/bar_chart.md)** | Categorical | Discrete categories, financial metrics, comparisons | .NET, WPF, React |
| **[HistogramPlot](./plots/histogram_plot.md)** | Distribution | Frequency distributions, statistical binning | .NET, WPF, React |
| **[BoxPlot](./plots/box_plot.md)** | Summary | Tukey quartiles ($Q_1$, Med, $Q_3$), whiskers, outliers | .NET, WPF, React |
| **[MatrixPlot](./plots/matrix_plot.md)** | Heatmap | 2D matrices, spatial fields, intensity maps | .NET, WPF, React |

---

## 🎯 Architecture At A Glance

```
                      ┌────────────────────────────────┐
                      │    Logical Mathematical Model   │
                      │  Ranges, Ticks, CoordinateMap  │
                      └───────────────┬────────────────┘
                                      │
          ┌───────────────────────────┼───────────────────────────┐
          │                           │                           │
          ▼                           ▼                           ▼
┌──────────────────┐        ┌──────────────────┐        ┌──────────────────┐
│   Luminy Core    │        │    Luminy.Wpf    │        │   Luminy.React   │
│   Pure C# SVG    │        │  StreamGeometry  │        │  Pure SVG (VDOM) │
│  Server/Reports  │        │ Hardware DirectX │        │ Interactive Web  │
└──────────────────┘        └──────────────────┘        └──────────────────┘
```
