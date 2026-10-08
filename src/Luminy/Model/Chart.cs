using System;
using System.IO;
using System.Text;

namespace Luminy.Model
{
    /// <summary>
    /// A container for rendering one or more Displays as an HTML figure with responsive column layout.
    /// </summary>
    /// <param name="rootPath">The base directory where generated SVG files are saved.</param>
    /// <param name="displays">The displays to include in this chart figure.</param>
    public class Chart(string rootPath, params Display[] displays)
    {
        /// <summary> Gets the displays contained in this chart. </summary>
        public Display[] Displays { get; } = displays;

        /// <summary> Gets or sets the number of columns in the responsive grid (default is 2). </summary>
        public int Columns { get; set; } = 2;

        /// <summary> Generates the HTML representation of this chart figure and writes SVGs to disk. </summary>
        public string ToHtml()
        {
            var html = new StringBuilder();

            html.AppendLine("<div class=\"chart-container\">");

            foreach (var display in Displays)
            {
                var svg = display.ToSvg();
                var fileName = SaveSvgToFile(svg);

                html.AppendLine("<div class=\"chart-item\">");
                html.AppendLine($"<img src=\"charts/{fileName}\" alt=\"Chart\" />");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");

            // Add CSS for responsive layout
            html.AppendLine("<style>");
            html.AppendLine(".chart-container { display: flex; flex-wrap: wrap; }");
            html.AppendLine($".chart-item {{ flex: 1 0 {100 / Columns}%; box-sizing: border-box; padding: 10px; }}");
            html.AppendLine("@media (max-width: 768px) { .chart-item { flex: 1 0 100%; } }");
            html.AppendLine("</style>");

            return html.ToString();
        }

        private string SaveSvgToFile(string svgContent)
        {
            var fileName = $"{Guid.NewGuid():N}.svg";
            var filePath = Path.Combine(rootPath, "charts", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.Combine(rootPath, "charts"));

            File.WriteAllText(filePath, svgContent);
            return fileName;
        }
    }
}