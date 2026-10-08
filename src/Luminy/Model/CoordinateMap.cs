namespace Luminy.Model
{
    /// <summary>
    /// Transforms mathematical data coordinates (u, v) into SVG screen pixel coordinates (x, y).
    /// </summary>
    public delegate (float x, float y) CoordinateMap(float u, float v);
}