namespace Luminy.Imaging
{
    /// <summary>
    /// Specifies the scanline prediction filter used during PNG compression.
    /// </summary>
    public enum PngFilterMode : byte
    {
        /// <summary> Raw pixel indices. </summary>
        None = 0,

        /// <summary> Horizontal difference: Orig(x) - Orig(x - 1). </summary>
        Sub = 1,

        /// <summary> Vertical difference between rows: Orig(x) - PriorRow(x). </summary>
        Up = 2,

        /// <summary>
        /// Dynamically selects per scanline the filter (None, Sub, or Up) that minimizes entropy.
        /// </summary>
        Adaptive = 3
    }
}
