using System;
using System.IO;
using System.Text;

namespace Luminy.Model
{
    public class Chart(string rootPath, params Display[] displays)
    {
        public Display[] Displays { get; } = displays ?? throw new ArgumentNullException(nameof(displays));
        public int Columns { get; set; } = 2;

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