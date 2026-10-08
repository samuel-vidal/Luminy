namespace Luminy.Model
{
    public record Color(float Red, float Green, float Blue, float Alpha = 1f)
    {
        public override string ToString()
        {
            return $"rgba({(int)(Red * 255)}, {(int)(Green * 255)}, {(int)(Blue * 255)}, {Alpha:N2})";
        }

        public static Color Lerp(Color a, Color b, float x)
        {
            return new Color(
                float.Lerp(a.Red, b.Red, x),
                float.Lerp(a.Green, b.Green, x),
                float.Lerp(a.Blue, b.Blue, x),
                float.Lerp(a.Alpha, b.Alpha, x));
        }

        public static Color Black = new(0, 0, 0);
        public static Color White = new(1, 1, 1);
        public static Color Transparent = new(0, 0, 0, 0);
    }
}