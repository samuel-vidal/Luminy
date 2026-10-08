using System;
using System.Linq;

namespace Luminy.Model
{
    public record Range(float Min, float Max)
    {
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