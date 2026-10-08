namespace Luminy.Model
{
    /// <summary>
    /// Represents an RGBA color with normalized float components between 0.0 and 1.0.
    /// </summary>
    public record Color(float Red, float Green, float Blue, float Alpha = 1f)
    {
        public override string ToString()
        {
            return $"rgba({(int)(Red * 255)}, {(int)(Green * 255)}, {(int)(Blue * 255)}, {Alpha:N2})";
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        public static Color Lerp(Color a, Color b, float x)
        {
            return new Color(
                float.Lerp(a.Red, b.Red, x),
                float.Lerp(a.Green, b.Green, x),
                float.Lerp(a.Blue, b.Blue, x),
                float.Lerp(a.Alpha, b.Alpha, x));
        }

        /// <summary> Pure black color. </summary>
        public static readonly Color Black = new(0, 0, 0);

        /// <summary> Pure white color. </summary>
        public static readonly Color White = new(1, 1, 1);

        /// <summary> Completely transparent color. </summary>
        public static readonly Color Transparent = new(0, 0, 0, 0);
    }
}