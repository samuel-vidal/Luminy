namespace Luminy.Imaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Unmanaged, aligned 2D pixel buffer allocated outside the GC heap.
    /// Supports direct, unpinned native pointer access with cache-aligned strides.
    /// </summary>
    /// <typeparam name="TPixel">The unmanaged pixel type.</typeparam>
    public sealed unsafe class BitmapBuffer<TPixel> : IDisposable where TPixel : unmanaged
    {
        private readonly nint byteStride;
        private TPixel* pointer;
        private bool disposed;

        /// <summary> Gets the width in pixels. </summary>
        public nint Width { get; }

        /// <summary> Gets the height in pixels. </summary>
        public nint Height { get; }

        /// <summary> Gets the scanline byte stride (including alignment padding). </summary>
        public nint Stride => byteStride;

        /// <summary> Gets the raw pointer to the first pixel. </summary>
        public TPixel* Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => pointer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BitmapBuffer{TPixel}"/> with aligned unmanaged memory.
        /// </summary>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="byteAlignment">Memory alignment in bytes (default 64 bytes).</param>
        public BitmapBuffer(nint width, nint height, uint byteAlignment = 64)
        {
            Width = width;
            Height = height;

            var rowBytes = width * sizeof(TPixel);
            var align = (nint)byteAlignment;
            byteStride = (rowBytes + (align - 1)) & ~(align - 1);

            var totalBytes = (nuint)(byteStride * height);
            pointer = (TPixel*)NativeMemory.AlignedAlloc(totalBytes, byteAlignment);
            NativeMemory.Clear(pointer, totalBytes);
        }

        /// <summary>
        /// Gets a reference to the pixel at the specified coordinate.
        /// </summary>
        /// <param name="u">The horizontal coordinate (column).</param>
        /// <param name="v">The vertical coordinate (row).</param>
        public ref TPixel this[nint u, nint v]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckAccess(u, v);
                var row = (byte*)pointer + (v * byteStride);
                return ref ((TPixel*)row)[u];
            }
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckAccess(nint u, nint v)
        {
            if (pointer == null)
            {
                throw new ObjectDisposedException(nameof(BitmapBuffer<TPixel>));
            }

            if (u < 0 || u >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(u), $"Coordinate u ({u}) must be in range [0, {Width}).");
            }

            if (v < 0 || v >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(v), $"Coordinate v ({v}) must be in range [0, {Height}).");
            }
        }

        /// <summary>
        /// Gets a reference to the pixel at the specified 32-bit integer coordinate.
        /// </summary>
        /// <param name="u">The horizontal coordinate (column).</param>
        /// <param name="v">The vertical coordinate (row).</param>
        public ref TPixel this[int u, int v]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[(nint)u, (nint)v];
        }

        /// <summary>
        /// Releases the unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                if (pointer != null)
                {
                    NativeMemory.AlignedFree(pointer);
                    pointer = null;
                    GC.SuppressFinalize(this);
                }
                disposed = true;
            }
        }

        ~BitmapBuffer() => Dispose();
    }
}
