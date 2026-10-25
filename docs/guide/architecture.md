# Architecture & Unified Design

Luminy unifies scientific plotting across C# (.NET), WPF, and React using the same retained-mode architectural foundations.

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

---

## The Retained-Mode Paradigm

Traditional plotting libraries often rely on imperative canvas rendering (`ctx.beginPath()`, `ctx.stroke()`). While fast for raw pixels, canvas rendering destroys the semantic scene graph, making accessibility, crisp vector scaling, and declarative framework integration difficult.

Luminy treats all charts as declarative retained scene graphs:
- In **React**, each plot is an SVG virtual DOM component wrapped in a `<Display>` parent.
- In **WPF**, each plot is a WPF `FrameworkElement` drawing into hardware-accelerated `StreamGeometry.Freeze()`.
- In **.NET Core**, each plot is an immutable C# record serialized to clean, standard W3C SVG strings.

---

## Synchronous Single-Pass Child Inspection

In `Luminy.React`, the `<Display>` component computes coordinate scales **synchronously during the initial render pass** without causing multi-phase layout thrashing or double-renders.

Each plot component defines a static descriptor:
```ts
export interface PlotComponent<P> {
  (props: P): React.ReactElement | null;
  getRange?: (props: P) => { xRange: Range; yRange: Range };
  getLegendItems?: (props: P, fallbackColor: string) => LegendItem[];
}
```

The parent `<Display>` iterates over `props.children` synchronously, reads `child.type.getRange(child.props)`, combines the bounding boxes, calculates "nice" tick intervals, and injects the coordinate mapper into `DisplayContext`.

---

## 100% DRY Algorithm Sharing

The core mathematical algorithms are identical across all targets:
- **Heckbert / Wilkinson Nice Ticks**: Computes human-friendly round tick intervals ($1, 2, 5 \times 10^n$).
- **Bounding Box Merging**: Expands empty or single-point ranges with padding factors.
- **Tukey Quartile Distribution**: $Q_1$, Median, $Q_3$, and $1.5 \times \text{IQR}$ outlier detection.
- **Color Interpolation**: Linear RGB interpolation for 2D density heatmaps.
