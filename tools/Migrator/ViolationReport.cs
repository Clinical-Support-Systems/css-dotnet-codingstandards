namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Report of violations found in a codebase.
/// </summary>
public sealed class ViolationReport
{
    public required string SolutionPath { get; init; }
    public required int TotalViolations { get; init; }
    public required List<Violation> Violations { get; init; }

    /// <summary>
    /// Gets violations grouped by stage.
    /// </summary>
    public Dictionary<string, List<Violation>> GetViolationsByStage()
    {
        return this.Violations.GroupBy(v => v.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets violations grouped by file.
    /// </summary>
    public Dictionary<string, List<Violation>> GetViolationsByFile()
    {
        return this.Violations.GroupBy(v => v.FilePath)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Gets count of auto-fixable violations.
    /// </summary>
    public int GetAutoFixableCount()
    {
        return this.Violations.Count(v => v.IsAutoFixable);
    }
}
