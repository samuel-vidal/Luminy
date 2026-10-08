using System;
using Luminy.Utils;
using System.Text;

namespace Luminy.Builders
{
    /// <summary>
    /// Base builder for generating formatted XML/SVG elements with disposable tags.
    /// </summary>
    public class XmlBuilder
    {
        protected readonly StringBuilder sb = new();

        protected static string EscapeXml(string? text)
        {
            if (text == null) return string.Empty;
            return text.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        /// <summary> Appends a self-closing XML tag with attributes. </summary>
        public void Tag(string name, params (string key, string value)[] attributes)
        {
            sb.Append($"<{name}");
            foreach (var (key, value) in attributes)
            {
                sb.Append($" {key}=\"{EscapeXml(value)}\"");
            }
            sb.Append("/>");
        }

        /// <summary> Opens an XML tag with attributes and returns an IDisposable that closes the tag. </summary>
        public IDisposable OpenTag(string name, params (string key, string value)[] attributes)
        {
            sb.Append($"<{name}");
            foreach (var (key, value) in attributes)
            {
                sb.Append($" {key}=\"{EscapeXml(value)}\"");
            }
            sb.Append(">");

            return new AnonymousDisposable(() => sb.Append($"</{name}>"));
        }

        /// <summary> Appends XML-escaped text content. </summary>
        public void Append(string text) => sb.Append(EscapeXml(text));

        /// <summary> Returns the generated XML string. </summary>
        public override string ToString() => sb.ToString();
    }
}