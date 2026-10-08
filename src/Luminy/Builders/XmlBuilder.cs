using System;
using Luminy.Utils;
using System.Text;

namespace Luminy.Builders
{
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

        public void Tag(string name, params (string key, string value)[] attributes)
        {
            sb.Append($"<{name}");
            foreach (var (key, value) in attributes)
            {
                sb.Append($" {key}=\"{EscapeXml(value)}\"");
            }
            sb.Append("/>");
        }

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

        public void Append(string text) => sb.Append(EscapeXml(text));

        public override string ToString() => sb.ToString();
    }
}