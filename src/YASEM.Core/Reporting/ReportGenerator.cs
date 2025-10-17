using System.Collections.Generic;
using System.IO;
using System.Text;
using YASEM.Core.Interfaces;
using YASEM.Core.Models;
using YASEM.Core.Validators;

namespace YASEM.Core.Reporting
{
    public class ReportGenerator : IReportGenerator
    {
        public void Generate(List<ValidationResult> results, string outputPath)
        {
            var builder = new StringBuilder();

            builder.AppendLine("<html>");
            builder.AppendLine("<head>");
            builder.AppendLine("<title>YASEM Validation Report</title>");
            builder.AppendLine("<style>");
            builder.AppendLine("body { font-family: sans-serif; }");
            builder.AppendLine("table { border-collapse: collapse; width: 100%; }");
            builder.AppendLine("th, td { border: 1px solid #dddddd; text-align: left; padding: 8px; }");
            builder.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            builder.AppendLine(".pass { color: green; }");
            builder.AppendLine(".fail { color: red; }");
            builder.AppendLine("</style>");
            builder.AppendLine("</head>");
            builder.AppendLine("<body>");
            builder.AppendLine("<h1>YASEM Validation Report</h1>");
            builder.AppendLine("<table>");
            builder.AppendLine("<tr>");
            builder.AppendLine("<th>Status</th>");
            builder.AppendLine("<th>Actual Value</th>");
            builder.AppendLine("</tr>");

            foreach (var result in results)
            {
                builder.AppendLine("<tr>");
                var statusClass = result.Status == Result.Pass ? "pass" : "fail";
                builder.AppendLine($"<td><span class='{statusClass}'>{result.Status}</span></td>");
                builder.AppendLine($"<td>{result.Actual}</td>");
                builder.AppendLine("</tr>");
            }

            builder.AppendLine("</table>");
            builder.AppendLine("</body>");
            builder.AppendLine("</html>");

            File.WriteAllText(outputPath, builder.ToString());
        }
    }
}
