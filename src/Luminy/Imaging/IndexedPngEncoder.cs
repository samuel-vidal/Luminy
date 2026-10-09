namespace Luminy.Imaging
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;

    /// <summary>
    /// High-performance, zero-dependency PNG encoder for 8-bit indexed images (LUT256).
    /// </summary>
    public static class IndexedPngEncoder
    {
        private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        private static readonly byte[] IhdrType = [0x49, 0x48, 0x44, 0x52]; // IHDR
        private static readonly byte[] PlteType = [0x50, 0x4C, 0x54, 0x45]; // PLTE
        private static readonly byte[] IdatType = [0x49, 0x44, 0x41, 0x54]; // IDAT
        private static readonly byte[] IendType = [0x49, 0x45, 0x4E, 0x44]; // IEND

        /// <summary>
        /// Encodes an 8-bit indexed <see cref="BitmapBuffer{Byte}"/> with a 256-color palette to a stream.
        /// </summary>
        /// <param name="buffer">The unmanaged pixel index buffer.</param>
        /// <param name="palette">The color palette (up to 256 colors).</param>
        /// <param name="output">The destination stream.</param>
        /// <param name="options">Optional encoding configuration.</param>
        public static unsafe void Encode(
            BitmapBuffer<byte> buffer,
            ReadOnlySpan<PngColor> palette,
            Stream output,
            PngEncodingOptions? options = null)
        {
            options ??= PngEncodingOptions.Default;

            output.Write(PngSignature);

            WriteIhdr(output, (int)buffer.Width, (int)buffer.Height);
            WritePlte(output, palette);
            WriteIdat(output, buffer, options);
            WriteIend(output);
        }

        /// <summary>
        /// Quantizes a 2D <see cref="Heatmap"/> and encodes it as an 8-bit indexed PNG into a stream.
        /// </summary>
        /// <param name="heatmap">The heatmap specification and tensor values.</param>
        /// <param name="output">The destination stream.</param>
        /// <param name="options">Optional encoding configuration.</param>
        public static void Encode(
            Heatmap heatmap,
            Stream output,
            PngEncodingOptions? options = null)
        {
            var rows = heatmap.Values.GetLength(0);
            var cols = heatmap.Values.GetLength(1);

            DetermineRange(heatmap, rows, cols, out var min, out var max);
            var range = max - min;

            using var buffer = new BitmapBuffer<byte>(cols, rows);

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var val = heatmap.Values[r, c];
                    if (!float.IsFinite(val))
                    {
                        buffer[c, r] = 0;
                        continue;
                    }

                    var normalized = range > 1e-6f ? (val - min) / range : 0f;
                    normalized = Math.Clamp(normalized, 0f, 1f);
                    buffer[c, r] = (byte)Math.Clamp((int)(normalized * 255.999f), 0, 255);
                }
            }

            var lut = PngColor.CreateLut256(heatmap.ColorScale);
            Encode(buffer, lut, output, options);
        }

        /// <summary>
        /// Encodes a <see cref="Heatmap"/> and saves it directly to a file.
        /// </summary>
        public static void Save(string filePath, Heatmap heatmap, PngEncodingOptions? options = null)
        {
            using var file = File.Create(filePath);
            Encode(heatmap, file, options);
        }

        /// <summary>
        /// Encodes a <see cref="BitmapBuffer{Byte}"/> and saves it directly to a file.
        /// </summary>
        public static void Save(
            string filePath,
            BitmapBuffer<byte> buffer,
            ReadOnlySpan<PngColor> palette,
            PngEncodingOptions? options = null)
        {
            using var file = File.Create(filePath);
            Encode(buffer, palette, file, options);
        }

        private static void WriteIhdr(Stream stream, int width, int height)
        {
            Span<byte> data = stackalloc byte[13];
            BinaryPrimitives.WriteInt32BigEndian(data[0..4], width);
            BinaryPrimitives.WriteInt32BigEndian(data[4..8], height);
            data[8] = 8; // bit depth: 8
            data[9] = 3; // color type: 3 (indexed palette)
            data[10] = 0; // compression: deflate
            data[11] = 0; // filter: standard
            data[12] = 0; // interlace: none

            WriteChunk(stream, IhdrType, data);
        }

        private static void WritePlte(Stream stream, ReadOnlySpan<PngColor> palette)
        {
            var paletteBytes = MemoryMarshal.AsBytes(palette[..Math.Min(palette.Length, 256)]);
            WriteChunk(stream, PlteType, paletteBytes);
        }

        private static unsafe void WriteIdat(Stream stream, BitmapBuffer<byte> buffer, PngEncodingOptions options)
        {
            using var compressedStream = new MemoryStream();

            using (var zlib = new ZLibStream(compressedStream, options.CompressionLevel, leaveOpen: true))
            {
                var width = (int)buffer.Width;
                var height = (int)buffer.Height;
                var stride = buffer.Stride;
                var rawPtr = (byte*)buffer.Pointer;

                var lineBuffer = ArrayPool<byte>.Shared.Rent(width + 1);
                var subBuffer = options.FilterMode == PngFilterMode.Adaptive ? ArrayPool<byte>.Shared.Rent(width + 1) : null;
                var upBuffer = options.FilterMode == PngFilterMode.Adaptive ? ArrayPool<byte>.Shared.Rent(width + 1) : null;

                try
                {
                    for (var v = 0; v < height; v++)
                    {
                        var currRow = rawPtr + (v * stride);
                        var prevRow = v > 0 ? rawPtr + ((v - 1) * stride) : null;

                        ApplyFilter(currRow, prevRow, width, options.FilterMode, lineBuffer, subBuffer, upBuffer, out var selectedBuffer);
                        zlib.Write(selectedBuffer.AsSpan(0, width + 1));
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(lineBuffer);
                    if (subBuffer != null) ArrayPool<byte>.Shared.Return(subBuffer);
                    if (upBuffer != null) ArrayPool<byte>.Shared.Return(upBuffer);
                }
            }

            var compressedData = compressedStream.GetBuffer().AsSpan(0, (int)compressedStream.Length);
            WriteChunk(stream, IdatType, compressedData);
        }

        private static unsafe void ApplyFilter(
            byte* currRow,
            byte* prevRow,
            int width,
            PngFilterMode filterMode,
            byte[] lineBuffer,
            byte[]? subBuffer,
            byte[]? upBuffer,
            out byte[] selectedBuffer)
        {
            switch (filterMode)
            {
                case PngFilterMode.None:
                    lineBuffer[0] = 0;
                    new ReadOnlySpan<byte>(currRow, width).CopyTo(lineBuffer.AsSpan(1));
                    selectedBuffer = lineBuffer;
                    return;

                case PngFilterMode.Sub:
                    lineBuffer[0] = 1;
                    lineBuffer[1] = currRow[0];
                    for (var i = 1; i < width; i++)
                    {
                        lineBuffer[i + 1] = (byte)(currRow[i] - currRow[i - 1]);
                    }
                    selectedBuffer = lineBuffer;
                    return;

                case PngFilterMode.Up:
                    lineBuffer[0] = 2;
                    for (var i = 0; i < width; i++)
                    {
                        var up = prevRow != null ? prevRow[i] : (byte)0;
                        lineBuffer[i + 1] = (byte)(currRow[i] - up);
                    }
                    selectedBuffer = lineBuffer;
                    return;

                case PngFilterMode.Adaptive:
                default:
                    // Compute None
                    lineBuffer[0] = 0;
                    var noneSum = 0;
                    for (var i = 0; i < width; i++)
                    {
                        var val = currRow[i];
                        lineBuffer[i + 1] = val;
                        noneSum += (sbyte)val >= 0 ? val : -((sbyte)val);
                    }

                    // Compute Sub
                    subBuffer![0] = 1;
                    subBuffer[1] = currRow[0];
                    var subSum = (sbyte)currRow[0] >= 0 ? currRow[0] : -((sbyte)currRow[0]);
                    for (var i = 1; i < width; i++)
                    {
                        var diff = (byte)(currRow[i] - currRow[i - 1]);
                        subBuffer[i + 1] = diff;
                        subSum += (sbyte)diff >= 0 ? diff : -((sbyte)diff);
                    }

                    // Compute Up
                    upBuffer![0] = 2;
                    var upSum = 0;
                    for (var i = 0; i < width; i++)
                    {
                        var up = prevRow != null ? prevRow[i] : (byte)0;
                        var diff = (byte)(currRow[i] - up);
                        upBuffer[i + 1] = diff;
                        upSum += (sbyte)diff >= 0 ? diff : -((sbyte)diff);
                    }

                    // Select filter with minimal absolute sum
                    if (subSum <= noneSum && subSum <= upSum)
                    {
                        selectedBuffer = subBuffer;
                    }
                    else if (upSum <= noneSum && upSum <= subSum)
                    {
                        selectedBuffer = upBuffer;
                    }
                    else
                    {
                        selectedBuffer = lineBuffer;
                    }
                    return;
            }
        }

        private static void WriteIend(Stream stream)
        {
            WriteChunk(stream, IendType, ReadOnlySpan<byte>.Empty);
        }

        private static void WriteChunk(Stream stream, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
        {
            Span<byte> header = stackalloc byte[8];
            BinaryPrimitives.WriteInt32BigEndian(header[..4], data.Length);
            type.CopyTo(header[4..8]);
            stream.Write(header);

            if (!data.IsEmpty)
            {
                stream.Write(data);
            }

            var crc = Crc32.Compute(type, data);
            Span<byte> crcBytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
            stream.Write(crcBytes);
        }

        private static void DetermineRange(Heatmap heatmap, int rows, int cols, out float min, out float max)
        {
            min = heatmap.Min ?? float.MaxValue;
            max = heatmap.Max ?? float.MinValue;

            if (heatmap.Min.HasValue && heatmap.Max.HasValue) return;

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var val = heatmap.Values[r, c];
                    if (!float.IsFinite(val)) continue;

                    if (!heatmap.Min.HasValue && val < min) min = val;
                    if (!heatmap.Max.HasValue && val > max) max = val;
                }
            }

            if (min > max)
            {
                min = 0f;
                max = 1f;
            }
        }
    }
}
