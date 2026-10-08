using System;
using System.Collections.Generic;
using Luminy.Model;

namespace Luminy.Builders
{
    /// <summary>
    /// Specialized builder for generating SVG vector graphics.
    /// </summary>
    public class SvgBuilder : XmlBuilder
    {
        /// <summary> Opens the root svg element. </summary>
        public IDisposable OpenSvg(float width, float height)
        {
            return OpenTag("svg", ("width", width.ToString("F3")), ("height", height.ToString("F3")),
                ("xmlns", "http://www.w3.org/2000/svg"));
        }

        /// <summary> Draws a rectangle. </summary>
        public void Rect(float x, float y, float width, float height, Color? fill = null, Color? stroke = null, float strokeWidth = 1f)
        {
            Tag("rect",
                ("x", x.ToString("F3")),
                ("y", y.ToString("F3")),
                ("width", width.ToString("F3")),
                ("height", height.ToString("F3")),
                ("fill", fill?.ToString() ?? "none"),
                ("stroke", stroke?.ToString() ?? "black"),
                ("stroke-width", strokeWidth.ToString("F3")));
        }

        /// <summary> Draws a circle. </summary>
        public void Circle(float cx, float cy, float r, Color? fill = null, Color? stroke = null, float strokeWidth = 1f)
        {
            Tag("circle",
                ("cx", cx.ToString("F3")),
                ("cy", cy.ToString("F3")),
                ("r", r.ToString("F3")),
                ("fill", fill?.ToString() ?? "none"),
                ("stroke", stroke?.ToString() ?? "black"),
                ("stroke-width", strokeWidth.ToString("F3")));
        }

        /// <summary> Draws a straight line. </summary>
        public void Line(float x1, float y1, float x2, float y2, Color? stroke = null, float strokeWidth = 1f)
        {
            Tag("line", ("x1", x1.ToString("F3")), ("y1", y1.ToString("F3")),
                ("x2", x2.ToString("F3")), ("y2", y2.ToString("F3")),
                ("stroke", stroke?.ToString() ?? "black"), ("stroke-width", strokeWidth.ToString("F3")));
        }

        /// <summary> Draws a custom SVG path. </summary>
        public void Path(string d, Color? fill = null, Color? stroke = null, float strokeWidth = 1)
        {
            Tag("path",
                ("d", d),
                ("fill", fill?.ToString() ?? "none"),
                ("stroke", stroke?.ToString() ?? "black"),
                ("stroke-width", strokeWidth.ToString("F3")));
        }

        public void Text(string content, float x, float y,
            string? fontFamily = null,
            float? fontSize = null,
            string? textAnchor = null,
            Color? fill = null,
            Color? stroke = null,
            string? fontWeight = null,
            string? transform = null)
        {
            var attributes = new List<(string, string)>
            {
                ("x", x.ToString("F3")),
                ("y", y.ToString("F3"))
            };

            if (fontFamily != null) attributes.Add(("font-family", fontFamily));
            if (fontSize != null) attributes.Add(("font-size", fontSize.Value.ToString("F3")));
            if (textAnchor != null) attributes.Add(("text-anchor", textAnchor));
            if (fill != null) attributes.Add(("fill", fill.ToString()));
            if (stroke != null) attributes.Add(("stroke", stroke.ToString()));
            if (fontWeight != null) attributes.Add(("font-weight", fontWeight));
            if (transform != null) attributes.Add(("transform", transform));

            using (OpenTag("text", attributes.ToArray()))
            {
                Append(content);
            }
        }

        public IDisposable OpenGroup()
        {
            return OpenTag("g");
        }

        public IDisposable OpenClipPath(string id)
        {
            return OpenTag("clipPath", ("id", id));
        }

        public void UseClipPath(string id)
        {
            Tag("clipPath", ("id", id));
        }
    }
}