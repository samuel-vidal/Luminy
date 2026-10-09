namespace Luminy.Imaging
{
    using System;
    using Luminy.Model;

    /// <summary>
    /// Represents a normalized color scale over the [0.0, 1.0] range, defined by piece-wise linear
    /// interpolation between gradient control points. Steps must be sorted in ascending order by their
    /// <see cref="GradientStep.Value"/>.
    /// </summary>
    public sealed record ColorScale
    {
        private readonly GradientStep[] steps;

        /// <summary>
        /// Gets the gradient control points.
        /// </summary>
        public GradientStep[] Steps => steps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorScale"/> class.
        /// </summary>
        /// <param name="steps">The gradient steps sorted in ascending order by value.</param>
        public ColorScale(params GradientStep[] steps)
        {
            this.steps = steps.Length == 0
                ? [new GradientStep(0f, Color.Black), new GradientStep(1f, Color.White)]
                : steps;
        }

        /// <summary>
        /// Evaluates the interpolated color at a given normalized position using binary search interpolation.
        /// </summary>
        /// <param name="value">The normalized position [0.0, 1.0] along the gradient scale.</param>
        public Color this[float value]
        {
            get
            {
                if (steps.Length == 1 || value <= steps[0].Value)
                {
                    return steps[0].Color;
                }

                if (value >= steps[^1].Value)
                {
                    return steps[^1].Color;
                }

                var low = 0;
                var high = steps.Length - 1;

                while (high - low > 1)
                {
                    var mid = (low + high) / 2;
                    if (steps[mid].Value <= value)
                    {
                        low = mid;
                    }
                    else
                    {
                        high = mid;
                    }
                }

                var stepA = steps[low];
                var stepB = steps[high];
                var range = stepB.Value - stepA.Value;
                var t = range > 1e-6f ? (value - stepA.Value) / range : 0f;

                return Color.Lerp(stepA.Color, stepB.Color, t);
            }
        }

        /// <summary>
        /// Gets the interpolated color for the specified value.
        /// </summary>
        public Color GetColor(float value) => this[value];

        /// <summary>
        /// Sequential colormap from deep blue to white.
        /// </summary>
        public static ColorScale BlueWhite { get; } = new(
            new GradientStep(0f, new Color(0.04f, 0.16f, 0.47f)),
            new GradientStep(1f, new Color(1f, 1f, 1f))
        );

        /// <summary>
        /// Diverging colormap from orange to white (center) to blue.
        /// </summary>
        public static ColorScale OrangeWhiteBlue { get; } = new(
            new GradientStep(0.0f, new Color(0.90f, 0.38f, 0.00f)),
            new GradientStep(0.5f, new Color(1.00f, 1.00f, 1.00f)),
            new GradientStep(1.0f, new Color(0.02f, 0.44f, 0.69f))
        );

        /// <summary>
        /// Perceptually uniform colormap from black to purple, orange, and bright yellow.
        /// </summary>
        public static ColorScale Inferno { get; } = new(
            new GradientStep(0.00f, new Color(0.000f, 0.000f, 0.004f)),
            new GradientStep(0.25f, new Color(0.341f, 0.063f, 0.427f)),
            new GradientStep(0.50f, new Color(0.733f, 0.224f, 0.337f)),
            new GradientStep(0.75f, new Color(0.969f, 0.584f, 0.118f)),
            new GradientStep(1.00f, new Color(0.988f, 1.000f, 0.643f))
        );

        /// <summary>
        /// Linear grayscale colormap from black to white.
        /// </summary>
        public static ColorScale Grayscale { get; } = new(
            new GradientStep(0f, Color.Black),
            new GradientStep(1f, Color.White)
        );

        /// <summary>
        /// Perceptually uniform colormap from deep violet to teal, green, and bright yellow.
        /// </summary>
        public static ColorScale Viridis { get; } = new(
            new GradientStep(0.00f, new Color(0.267f, 0.004f, 0.329f)),
            new GradientStep(0.25f, new Color(0.231f, 0.322f, 0.545f)),
            new GradientStep(0.50f, new Color(0.129f, 0.569f, 0.553f)),
            new GradientStep(0.75f, new Color(0.369f, 0.788f, 0.384f)),
            new GradientStep(1.00f, new Color(0.993f, 0.906f, 0.145f))
        );

        /// <summary>
        /// Perceptually uniform colormap from dark violet to magenta, orange, and bright yellow.
        /// </summary>
        public static ColorScale Plasma { get; } = new(
            new GradientStep(0.00f, new Color(0.051f, 0.031f, 0.529f)),
            new GradientStep(0.25f, new Color(0.494f, 0.012f, 0.659f)),
            new GradientStep(0.50f, new Color(0.796f, 0.278f, 0.471f)),
            new GradientStep(0.75f, new Color(0.973f, 0.584f, 0.255f)),
            new GradientStep(1.00f, new Color(0.941f, 0.976f, 0.129f))
        );

        /// <summary>
        /// High-contrast rainbow colormap with smooth transitions.
        /// </summary>
        public static ColorScale Turbo { get; } = new(
            new GradientStep(0.00f, new Color(0.190f, 0.072f, 0.232f)),
            new GradientStep(0.15f, new Color(0.176f, 0.428f, 0.887f)),
            new GradientStep(0.35f, new Color(0.137f, 0.769f, 0.694f)),
            new GradientStep(0.55f, new Color(0.635f, 0.890f, 0.212f)),
            new GradientStep(0.75f, new Color(0.984f, 0.627f, 0.149f)),
            new GradientStep(0.90f, new Color(0.898f, 0.263f, 0.071f)),
            new GradientStep(1.00f, new Color(0.480f, 0.015f, 0.014f))
        );
    }
}
