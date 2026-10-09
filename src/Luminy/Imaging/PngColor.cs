namespace Luminy.Imaging
{
    using System;
    using System.Runtime.InteropServices;
    using Luminy.Model;

    /// <summary>
    /// Blittable 24-bit RGB color representation matching the PNG PLTE chunk layout.
    /// </summary>
    /// <param name="Red">Red component (0 to 255).</param>
    /// <param name="Green">Green component (0 to 255).</param>
    /// <param name="Blue">Blue component (0 to 255).</param>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct PngColor(byte Red, byte Green, byte Blue)
    {
        /// <summary> Pure black color. </summary>
        public static readonly PngColor Black = new(0, 0, 0);

        /// <summary> Pure white color. </summary>
        public static readonly PngColor White = new(255, 255, 255);

        /// <summary>
        /// Converts a floating-point <see cref="Color"/> to a <see cref="PngColor"/>.
        /// </summary>
        public static PngColor FromColor(Color color) => new(
            (byte)Math.Clamp((int)Math.Round(color.Red * 255f), 0, 255),
            (byte)Math.Clamp((int)Math.Round(color.Green * 255f), 0, 255),
            (byte)Math.Clamp((int)Math.Round(color.Blue * 255f), 0, 255)
        );

        /// <summary>
        /// Samples a <see cref="ColorScale"/> at 256 evenly spaced positions to construct a 256-color LUT.
        /// </summary>
        public static PngColor[] CreateLut256(ColorScale scale)
        {
            var lut = new PngColor[256];
            for (var i = 0; i < 256; i++)
            {
                var t = i / 255.0f;
                lut[i] = FromColor(scale[t]);
            }
            return lut;
        }
    }
}
