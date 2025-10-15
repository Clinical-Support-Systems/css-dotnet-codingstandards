using System.Text;

namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Generates reports from violation data.
/// </summary>
public sealed class ReportGenerator
{
    /// <summary>
    /// Generates a text-based report.
    /// </summary>
    public string GenerateTextReport(ViolationReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("CSS.DotNet.CodingStandards Migration Report");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Solution: {report.SolutionPath}");
        sb.AppendLine($"Total Violations: {report.TotalViolations}");
        sb.AppendLine($"Auto-fixable: {report.GetAutoFixableCount()}");
        sb.AppendLine($"Manual: {report.TotalViolations - report.GetAutoFixableCount()}");
        sb.AppendLine();

        var byStage = report.GetViolationsByStage();
        sb.AppendLine("Violations by Stage:");
        sb.AppendLine("-".PadRight(80, '-'));

        foreach (var stage in Enum.GetValues<MigrationStage>())
        {
            var stageName = stage.ToString();
            if (byStage.TryGetValue(stageName, out var violations))
            {
                var autoFixable = violations.Count(v => v.IsAutoFixable);
                sb.AppendLine($"  {stageName,-15} {violations.Count,6} total, {autoFixable,6} auto-fixable");
            }
            else
            {
                sb.AppendLine($"  {stageName,-15} {0,6} total, {0,6} auto-fixable");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Top 10 Files by Violation Count:");
        sb.AppendLine("-".PadRight(80, '-'));

        var byFile = report.GetViolationsByFile()
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(10);

        foreach (var (file, violations) in byFile)
        {
            var fileName = Path.GetFileName(file);
            sb.AppendLine($"  {violations.Count,4} - {fileName}");
        }

        sb.AppendLine();
        sb.AppendLine("Top Violation Types:");
        sb.AppendLine("-".PadRight(80, '-'));

        var byId = report.Violations
            .GroupBy(v => new { v.Id, v.Title })
            .OrderByDescending(g => g.Count())
            .Take(15);

        foreach (var group in byId)
        {
            sb.AppendLine($"  {group.Count(),4} - {group.Key.Id}: {group.Key.Title}");
        }

        sb.AppendLine();
        sb.AppendLine("=".PadRight(80, '='));

        return sb.ToString();
    }

    /// <summary>
    /// Generates a GitHub PR description with before/after statistics.
    /// </summary>
    public string GeneratePRDescription(ViolationReport beforeReport, ViolationReport? afterReport, MigrationStage stage)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## Migration: {stage} Stage");
        sb.AppendLine();
        sb.AppendLine("This PR applies automatic fixes for the following stage of the CSS.DotNet.CodingStandards migration:");
        sb.AppendLine();

        var stageName = stage.ToString();
        var beforeStageViolations = beforeReport.GetViolationsByStage()
            .GetValueOrDefault(stageName, []);

        sb.AppendLine($"### Stage: {stage}");
        sb.AppendLine();

        switch (stage)
        {
            case MigrationStage.Formatting:
                sb.AppendLine("Fixes code formatting issues including indentation, spacing, braces, and file-scoped namespaces.");
                break;
            case MigrationStage.Naming:
                sb.AppendLine("Fixes naming convention violations.");
                break;
            case MigrationStage.Logic:
                sb.AppendLine("Fixes logic and code quality issues that can be automatically corrected.");
                break;
            case MigrationStage.Manual:
                sb.AppendLine("Issues requiring manual review and correction.");
                break;
        }

        sb.AppendLine();
        sb.AppendLine("### Statistics");
        sb.AppendLine();
        sb.AppendLine("| Metric | Before | After | Change |");
        sb.AppendLine("|--------|--------|-------|--------|");

        if (afterReport != null)
        {
            var afterStageViolations = afterReport.GetViolationsByStage()
                .GetValueOrDefault(stageName, []);
            var fixedCount = beforeStageViolations.Count - afterStageViolations.Count;
            sb.AppendLine($"| {stageName} Violations | {beforeStageViolations.Count} | {afterStageViolations.Count} | -{fixedCount} |");
            sb.AppendLine($"| Total Violations | {beforeReport.TotalViolations} | {afterReport.TotalViolations} | -{beforeReport.TotalViolations - afterReport.TotalViolations} |");
        }
        else
        {
            sb.AppendLine($"| {stageName} Violations | {beforeStageViolations.Count} | - | - |");
        }

        sb.AppendLine();
        sb.AppendLine("### Review Notes");
        sb.AppendLine();
        sb.AppendLine("- All changes were automatically generated using `dotnet format`");
        sb.AppendLine("- No manual changes were made");
        sb.AppendLine("- Verify that tests pass before merging");

        return sb.ToString();
    }

    /// <summary>
    /// Generates suppression suggestions for problematic areas.
    /// </summary>
    public string GenerateSuppressionSuggestions(ViolationReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Suggested Suppressions for Problematic Areas");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();

        var manualViolations = report.Violations
            .Where(v => !v.IsAutoFixable)
            .GroupBy(v => v.Id)
            .OrderByDescending(g => g.Count())
            .Take(10);

        foreach (var group in manualViolations)
        {
            var violation = group.First();
            sb.AppendLine($"Rule: {violation.Id} - {violation.Title}");
            sb.AppendLine($"Count: {group.Count()}");
            sb.AppendLine();
            sb.AppendLine("Option 1: Suppress in .editorconfig");
            sb.AppendLine("```editorconfig");
            sb.AppendLine($"# Temporarily disable {violation.Id} during migration");
            sb.AppendLine("[*.cs]");
            sb.AppendLine($"dotnet_diagnostic.{violation.Id}.severity = none");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Option 2: Suppress in specific files");
            sb.AppendLine("```editorconfig");
            var commonPath = FindCommonPath(group.Select(v => v.FilePath).ToList());
            if (!string.IsNullOrEmpty(commonPath))
            {
                sb.AppendLine($"[{commonPath}/**/*.cs]");
                sb.AppendLine($"dotnet_diagnostic.{violation.Id}.severity = none");
            }

            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FindCommonPath(List<string> paths)
    {
        if (paths.Count == 0)
        {
            return string.Empty;
        }

        var parts = paths[0].Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var commonParts = new List<string>();

        for (var i = 0; i < parts.Length - 1; i++) // -1 to exclude filename
        {
            var part = parts[i];
            if (paths.All(p => p.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Skip(i).FirstOrDefault() == part))
            {
                commonParts.Add(part);
            }
            else
            {
                break;
            }
        }

        return string.Join("/", commonParts);
    }
}
