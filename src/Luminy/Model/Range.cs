using System;
using System.Linq;

namespace Luminy.Model
{
    /// <summary>
    /// Represents a 1D numerical interval [Min, Max].
    /// </summary>
    public record Range(float Min, float Max)
    {
        /// <summary>
        /// Combines multiple ranges into a single enclosing range.
        /// </summary>
        public static Range Combine(params Range[] ranges)
        {
            if (ranges.Length == 0) return new Range(0, 0);

            var min = ranges[0].Min;
            var max = ranges[0].Max;

            foreach (var range in ranges.Skip(1))
            {
                min = Math.Min(min, range.Min);
                max = Math.Max(max, range.Max);
            }

            return new Range(min, max);
        }
    }
}