namespace Luminy.Imaging
{
    /// <summary>
    /// Pure data specification for rendering a 2D numerical array as a color-mapped heatmap.
    /// </summary>
    /// <param name="Values">The 2D numerical array.</param>
    /// <param name="ColorScale">The color scale used for mapping values to colors.</param>
    /// <param name="Min">Optional lower clamp bound. When null, computed automatically from data.</param>
    /// <param name="Max">Optional upper clamp bound. When null, computed automatically from data.</param>
    public sealed record Heatmap(
        float[,] Values,
        ColorScale ColorScale,
        float? Min = null,
        float? Max = null);
}
