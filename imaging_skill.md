# AI Skill: Luminy Imaging Library

`Luminy.Imaging` is a lightweight, zero-dependency image encoding module designed for high-performance AI tensor visualization, memory inspection, and scientific heatmaps in pure C# (.NET 9). It specializes in producing ultra-compact, 8-bit indexed PNG images (LUT256) with zero external graphic libraries.

---

## The Mental Model

The imaging architecture is partitioned into three decoupled layers:

1. **Layer 1: Domain & Color Space (`Heatmap`, `ColorScale`, `GradientStep`)**  
   Encapsulates high-level mathematical data and normalized color representations. A `ColorScale` models a piece-wise linear gradient over the normalized interval $[0.0, 1.0]$ using $O(\log N)$ dichotomous search and linear color interpolation (`Color.Lerp`). Built-in presets (`Inferno`, `Viridis`, `Plasma`, `Turbo`, `BlueWhite`, `OrangeWhiteBlue`, `Grayscale`) provide standardized scientific colormaps. A `Heatmap` pairs a 2D float tensor with a `ColorScale` and optional clamp boundaries.

2. **Layer 2: Unmanaged Memory & Buffer Abstraction (`BitmapBuffer<TPixel>`)**  
   Provides a cache-aligned 2D pixel buffer allocated outside the garbage collector via `NativeMemory.AlignedAlloc` and `NativeMemory.AlignedFree` (defaulting to 64-byte alignment). Exposes direct unpinned native pointers (`Pointer`) and strides (`Stride`) suitable for SIMD, OpenCL, or high-throughput tensor kernel interop without GC pressure. Includes inline `[u, v]` coordinate accessors with debug-time bounds checking.

3. **Layer 3: PNG Encoding Engine (`IndexedPngEncoder`, `PngColor`, `Crc32`)**  
   Implements an RFC 2083 compliant 8-bit indexed PNG streaming engine. Manages `IHDR`, `PLTE` (24-bit RGB palette), compressed `IDAT`, and `IEND` chunks. Compression is handled natively via `System.IO.Compression.ZLibStream`. Supports standard scanline prediction filters (`None`, `Sub` horizontal difference, `Up` vertical line difference, and `Adaptive` entropy minimization) using zero-allocation pooled line buffers.

---

## Core Data Types & Namespaces

```csharp
using Luminy.Imaging; // Contains Heatmap, ColorScale, BitmapBuffer, and IndexedPngEncoder
using Luminy.Model;   // Contains Color
```

---

## API Reference

### 1. Color Representation & Scales
- `new GradientStep(float Value, Color Color)`: A control point at a normalized position within $[0.0, 1.0]$.
- `new ColorScale(params GradientStep[] steps)`: Immutable color scale based on sorted control points.
  - `scale[float value]`: Indexer returning the interpolated `Color` for a normalized position.
  - `scale.GetColor(float value)`: Method alias for the indexer.
  - Presets: `ColorScale.Inferno`, `ColorScale.Viridis`, `ColorScale.Plasma`, `ColorScale.Turbo`, `ColorScale.BlueWhite`, `ColorScale.OrangeWhiteBlue`, `ColorScale.Grayscale`.

### 2. High-Level Data Model
- `new Heatmap(float[,] Values, ColorScale ColorScale, float? Min = null, float? Max = null)`:
  - Immutable specification for a 2D tensor heatmap. When `Min` and `Max` are omitted, bounds are computed automatically from finite tensor entries.

### 3. Native Buffer
- `new BitmapBuffer<TPixel>(nint width, nint height, uint byteAlignment = 64)`:
  - `buffer.Width`, `buffer.Height`: Dimensions in pixels.
  - `buffer.Stride`: Row byte stride, padded to the specified alignment boundary.
  - `buffer.Pointer`: Raw unmanaged pointer to the first pixel element.
  - `buffer[nint u, nint v]`, `buffer[int u, int v]`: Inline reference accessors (with debug-time bounds checks).
  - Implements `IDisposable` for deterministic memory release.

### 4. Palette & Encoding Options
- `PngColor(byte Red, byte Green, byte Blue)`: Blittable 24-bit RGB structure for palette chunks.
  - `PngColor.CreateLut256(ColorScale scale)`: Samples a `ColorScale` across 256 evenly spaced intervals to construct a complete LUT256.
- `PngEncodingOptions`:
  - `FilterMode`: `PngFilterMode.None`, `Sub`, `Up`, or `Adaptive`.
  - `CompressionLevel`: System compression level (e.g. `Optimal`, `Fastest`).
  - Presets: `PngEncodingOptions.Default`, `PngEncodingOptions.Fast`.

### 5. PNG Encoder
- `IndexedPngEncoder.Encode(Heatmap heatmap, Stream output, PngEncodingOptions? options = null)`: Quantizes a tensor and streams an 8-bit PNG.
- `IndexedPngEncoder.Save(string filePath, Heatmap heatmap, PngEncodingOptions? options = null)`: Encodes and writes a tensor heatmap to disk.
- `IndexedPngEncoder.Encode(BitmapBuffer<byte> buffer, ReadOnlySpan<PngColor> palette, Stream output, PngEncodingOptions? options = null)`: Encodes a raw indexed buffer.
- `IndexedPngEncoder.Save(string filePath, BitmapBuffer<byte> buffer, ReadOnlySpan<PngColor> palette, PngEncodingOptions? options = null)`: Saves a raw indexed buffer to disk.

---

## Examples

### 1. High-Level Tensor Heatmap Example (`float[,]`)

This approach maps floating-point matrices (e.g., weights, activations, attention maps) directly into a compressed PNG file.

```csharp
using System;
using Luminy.Imaging;

public class TensorHeatmapExample
{
    public void ExportAttentionMap(float[,] attentionWeights, string outputPath)
    {
        // 1. Define the heatmap pairing tensor data with a scientific color scale
        var heatmap = new Heatmap(
            Values: attentionWeights,
            ColorScale: ColorScale.Inferno,
            Min: 0.0f,
            Max: 1.0f
        );

        // 2. Encode and save directly to disk with optimal adaptive compression
        IndexedPngEncoder.Save(outputPath, heatmap, PngEncodingOptions.Default);
    }
}
```

---

### 2. Low-Level Native Buffer Example (`BitmapBuffer<byte>`)

This approach is suited for custom quantization pipelines, SIMD pre-processing, or hardware compute buffers that write raw pixel indices ($0..255$) directly into unmanaged memory.

```csharp
using System;
using System.IO;
using Luminy.Imaging;

public class LowLevelBufferExample
{
    public void ExportQuantizedBuffer(int width, int height, string outputPath)
    {
        // 1. Allocate a 64-byte aligned unmanaged buffer
        using var buffer = new BitmapBuffer<byte>(width, height);

        // 2. Direct unpinned access (via pointer or inline indexer)
        for (var v = 0; v < height; v++)
        {
            for (var u = 0; u < width; u++)
            {
                // Write pixel palette index (0..255)
                buffer[u, v] = (byte)((u * 255) / width);
            }
        }

        // 3. Generate a 256-color lookup table from a preset or custom scale
        var palette = PngColor.CreateLut256(ColorScale.Turbo);

        // 4. Save to disk with differential sub-filtering
        var options = new PngEncodingOptions
        {
            FilterMode = PngFilterMode.Sub
        };

        IndexedPngEncoder.Save(outputPath, buffer, palette, options);
    }
}
```
