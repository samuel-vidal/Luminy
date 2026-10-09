namespace Luminy.Imaging
{
    using Luminy.Model;

    /// <summary>
    /// A control point along a color gradient.
    /// </summary>
    /// <param name="Value">The position along the gradient scale.</param>
    /// <param name="Color">The color at this control point.</param>
    public readonly record struct GradientStep(float Value, Color Color);
}
