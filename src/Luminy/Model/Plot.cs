using Luminy.Builders;

namespace Luminy.Model
{
    /// <summary>
    /// Base record for all visualizations in Luminy.
    /// </summary>
    /// <param name="Label">A descriptive name for this data series, used in the legend.</param>
    /// <param name="Color">The primary color for this plot.</param>
    public abstract record Plot(string Label, Color Color)
    {
        /// <summary>
        /// Calculates the data range covered by this plot.
        /// </summary>
        /// <returns>The min/max ranges for both X and Y axes.</returns>
        public abstract (Range xRange, Range yRange) GetValueRange();

        /// <summary>
        /// Renders the plot into the provided SVG builder.
        /// </summary>
        /// <param name="svg">The SVG builder to write to.</param>
        /// <param name="map">The coordinate mapping function to transform data units to pixels.</param>
        public abstract void Render(SvgBuilder svg, CoordinateMap map);

        /// <summary>
        /// Helper to check if a point is valid (finite) before rendering.
        /// </summary>
        protected static bool IsValidPoint(float x, float y)
        {
            return float.IsFinite(x) && float.IsFinite(y);
        }
    }
}