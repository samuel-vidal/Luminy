namespace Luminy.Imaging
{
    using System.IO.Compression;

    /// <summary>
    /// Configuration options for the PNG encoder.
    /// </summary>
    public sealed record PngEncodingOptions
    {
        /// <summary> Gets the scanline filtering strategy. </summary>
        public PngFilterMode FilterMode { get; init; } = PngFilterMode.Adaptive;

        /// <summary> Gets the Deflate compression level. </summary>
        public CompressionLevel CompressionLevel { get; init; } = CompressionLevel.Optimal;

        /// <summary> Default options with adaptive filtering and optimal compression. </summary>
        public static PngEncodingOptions Default { get; } = new();

        /// <summary> Fast options with no filtering and fastest compression. </summary>
        public static PngEncodingOptions Fast { get; } = new()
        {
            FilterMode = PngFilterMode.None,
            CompressionLevel = CompressionLevel.Fastest
        };
    }
}
