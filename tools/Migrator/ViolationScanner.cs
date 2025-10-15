using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Scans a codebase for violations using Roslyn analyzers.
/// </summary>
public sealed class ViolationScanner
{
    private static readonly string[] FormattingRules = [
        "IDE0001", "IDE0004", "IDE0005", "IDE0007", "IDE0009", "IDE0011",
        "IDE0055", "IDE0161", "IDE2000", "IDE2001", "IDE2002", "IDE2003",
        "IDE2004", "IDE2005", "IDE2006"
    ];

    private static readonly string[] NamingRules = [
        "IDE1006", // Naming rule violation
    ];

    /// <summary>
    /// Scans a solution or project for violations.
    /// </summary>
    public async Task<ViolationReport> ScanAsync(string solutionOrProjectPath, CancellationToken cancellationToken = default)
    {
        var violations = new List<Violation>();

        Console.WriteLine($"Loading workspace: {solutionOrProjectPath}");
        using var workspace = MSBuildWorkspace.Create();

        workspace.WorkspaceFailed += (sender, args) =>
        {
            if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Warning)
            {
                Console.WriteLine($"Warning: {args.Diagnostic.Message}");
            }
        };

        Solution solution;
        if (solutionOrProjectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            solution = await workspace.OpenSolutionAsync(solutionOrProjectPath, cancellationToken: cancellationToken);
        }
        else
        {
            var project = await workspace.OpenProjectAsync(solutionOrProjectPath, cancellationToken: cancellationToken);
            solution = project.Solution;
        }

        Console.WriteLine($"Analyzing {solution.Projects.Count()} project(s)...");

        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"  Analyzing project: {project.Name}");
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null)
            {
                continue;
            }

            var diagnostics = compilation.GetDiagnostics(cancellationToken);
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Location == Location.None || !diagnostic.Location.IsInSource)
                {
                    continue;
                }

                // Filter for warning-level diagnostics (as per the README)
                if (diagnostic.Severity != DiagnosticSeverity.Warning && diagnostic.Severity != DiagnosticSeverity.Error)
                {
                    continue;
                }

                var lineSpan = diagnostic.Location.GetLineSpan();
                var category = CategorizeRule(diagnostic.Id);

                violations.Add(new Violation
                {
                    Id = diagnostic.Id,
                    Title = diagnostic.Descriptor.Title.ToString(),
                    FilePath = lineSpan.Path,
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    Severity = diagnostic.Severity.ToString(),
                    Category = category.ToString(),
                    IsAutoFixable = IsAutoFixable(diagnostic.Id, category)
                });
            }
        }

        return new ViolationReport
        {
            TotalViolations = violations.Count,
            Violations = violations,
            SolutionPath = solutionOrProjectPath
        };
    }

    private static string CategorizeRule(string ruleId)
    {
        if (FormattingRules.Contains(ruleId))
        {
            return MigrationStage.Formatting.ToString();
        }

        if (NamingRules.Contains(ruleId))
        {
            return MigrationStage.Naming.ToString();
        }

        // CA rules and other IDE rules that are typically auto-fixable
        if (ruleId.StartsWith("CA", StringComparison.Ordinal) || ruleId.StartsWith("IDE", StringComparison.Ordinal))
        {
            return MigrationStage.Logic.ToString();
        }

        return MigrationStage.Manual.ToString();
    }

    private static bool IsAutoFixable(string ruleId, string category)
    {
        // Formatting and many naming issues are auto-fixable
        if (category == MigrationStage.Formatting.ToString() || category == MigrationStage.Naming.ToString())
        {
            return true;
        }

        // Some logic rules are auto-fixable
        if (category == MigrationStage.Logic.ToString())
        {
            // Conservative approach: assume some are fixable
            return ruleId.StartsWith("IDE", StringComparison.Ordinal);
        }

        return false;
    }
}
