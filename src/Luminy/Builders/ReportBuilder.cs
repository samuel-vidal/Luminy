using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminy.Model;
using System.Text;

namespace Luminy.Builders
{
    /// <summary>
    /// A fluent builder for creating scientific HTML reports containing text, tables, and charts.
    /// </summary>
    public class ReportBuilder
    {
        private readonly string rootPath;
        private readonly string fileName;
        private readonly StringBuilder html = new();

        /// <summary>
        /// Initializes a new instance of the ReportBuilder.
        /// </summary>
        /// <param name="rootPath">The directory where the report and its assets will be saved.</param>
        /// <param name="fileName">The name of the HTML file (e.g., "index.html").</param>
        public ReportBuilder(string rootPath, string fileName)
        {
            this.rootPath = rootPath;
            this.fileName = fileName;

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"UTF-8\">");
            html.AppendLine("<title>Scientific Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; line-height: 1.6; }");
            html.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
            html.AppendLine("h3 { color: #7f8c8d; }");
            html.AppendLine("p { margin-bottom: 15px; }");
            html.AppendLine("code { background-color: #f8f9fa; padding: 2px 4px; border-radius: 3px; font-family: 'Courier New', monospace; }");
            html.AppendLine("pre { background-color: #f8f9fa; padding: 15px; border-radius: 5px; overflow-x: auto; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; }");
            html.AppendLine("figure { margin: 20px 0; text-align: center; }");
            html.AppendLine("figcaption { font-style: italic; color: #7f8c8d; margin-top: 5px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
        }

        /// <summary> Adds a level-1 title to the report. </summary>
        public ReportBuilder AddTitle(string title)
        {
            html.AppendLine($"<h1>{EscapeHtml(title)}</h1>");
            return this;
        }

        /// <summary> Adds a level-2 section header to the report. </summary>
        public ReportBuilder AddSection(string title)
        {
            html.AppendLine($"<h2>{EscapeHtml(title)}</h2>");
            return this;
        }

        /// <summary> Adds a level-3 subsection header to the report. </summary>
        public ReportBuilder AddSubsection(string title)
        {
            html.AppendLine($"<h3>{EscapeHtml(title)}</h3>");
            return this;
        }

        /// <summary> Adds a paragraph of text to the report. </summary>
        public ReportBuilder AddParagraph(string text)
        {
            html.AppendLine($"<p>{EscapeHtml(text)}</p>");
            return this;
        }

        /// <summary> Adds a formatted code block with optional syntax language to the report. </summary>
        public ReportBuilder AddCodeBlock(string code, string language = "")
        {
            html.AppendLine("<pre>");
            if (!string.IsNullOrEmpty(language))
            {
                html.AppendLine($"<code class=\"language-{language}\">");
            }
            else
            {
                html.AppendLine("<code>");
            }
            html.AppendLine(EscapeHtml(code));
            html.AppendLine("</code>");
            html.AppendLine("</pre>");
            return this;
        }

        /// <summary> Adds a table to the report. </summary>
        public ReportBuilder AddTable(string[,] data, string[]? headers = null)
        {
            html.AppendLine("<table>");

            if (headers != null)
            {
                html.AppendLine("<tr>");
                foreach (var header in headers)
                {
                    html.AppendLine($"<th>{EscapeHtml(header)}</th>");
                }
                html.AppendLine("</tr>");
            }

            for (var i = 0; i < data.GetLength(0); i++)
            {
                html.AppendLine("<tr>");
                for (var j = 0; j < data.GetLength(1); j++)
                {
                    html.AppendLine($"<td>{EscapeHtml(data[i, j])}</td>");
                }
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            return this;
        }

        /// <summary> Adds a chart (or multiple charts) to the report. </summary>
        public ReportBuilder AddChart(Chart chart, string caption = "")
        {
            var chartHtml = chart.ToHtml();
            html.AppendLine("<figure>");
            html.AppendLine(chartHtml);
            if (!string.IsNullOrEmpty(caption))
            {
                html.AppendLine($"<figcaption>{EscapeHtml(caption)}</figcaption>");
            }
            html.AppendLine("</figure>");
            return this;
        }

        /// <summary> Adds a single display directly as a chart figure to the report. </summary>
        public ReportBuilder AddChart(Display display, string caption = "")
        {
            return AddChart(new Chart(rootPath, display), caption);
        }

        /// <summary> Adds multiple displays arranged in columns as a chart figure to the report. </summary>
        public ReportBuilder AddChart(IEnumerable<Display> displays, int columns = 2, string caption = "")
        {
            var chart = new Chart(rootPath, displays.ToArray()) { Columns = columns };
            return AddChart(chart, caption);
        }

        /// <summary> Adds multiple displays arranged in columns as a chart figure to the report. </summary>
        public ReportBuilder AddChart(string caption, int columns, params Display[] displays)
        {
            var chart = new Chart(rootPath, displays) { Columns = columns };
            return AddChart(chart, caption);
        }

        /// <summary> Adds an image to the report. </summary>
        public ReportBuilder AddImage(string imagePath, string altText = "", string caption = "")
        {
            html.AppendLine("<figure>");
            html.AppendLine($"<img src=\"{imagePath}\" alt=\"{EscapeHtml(altText)}\" />");
            if (!string.IsNullOrEmpty(caption))
            {
                html.AppendLine($"<figcaption>{EscapeHtml(caption)}</figcaption>");
            }
            html.AppendLine("</figure>");
            return this;
        }

        /// <summary> Adds an ordered or unordered list of items to the report. </summary>
        public ReportBuilder AddList(IEnumerable<string> items, bool ordered = false)
        {
            var tag = ordered ? "ol" : "ul";
            html.AppendLine($"<{tag}>");
            foreach (var item in items)
            {
                html.AppendLine($"<li>{EscapeHtml(item)}</li>");
            }
            html.AppendLine($"</{tag}>");
            return this;
        }

        /// <summary> Finalizes the report and writes it to the disk. </summary>
        public ReportBuilder Build()
        {
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            Directory.CreateDirectory(rootPath);
            var reportPath = Path.Combine(rootPath, fileName);
            File.WriteAllText(reportPath, html.ToString());
            return this;
        }

        private string EscapeHtml(string text)
        {
            if (text == null) return string.Empty;
            return text.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }
}