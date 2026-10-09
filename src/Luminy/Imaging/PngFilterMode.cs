namespace Luminy.Imaging
{
    /// <summary>
    /// Specifies the scanline prediction filter used during PNG compression.
    /// </summary>
    public enum PngFilterMode : byte
    {
        /// <summary> Filter 0: None (raw pixel indices). </summary>
        None = 0,

        /// <summary> Filter 1: Sub (horizontal difference: Orig(x) - Orig(x - 1)). </summary>
        Sub = 1,

        /// <summary> Filter 2: Up (vertical difference between rows: Orig(x) - PriorRow(x)). </summary>
        Up = 2,

        /// <summary>
        /// Dynamically selects per scanline the filter (None, Sub, or Up) that minimizes entropy.
        /// </summary>
        Adaptive = 3
    }
}
