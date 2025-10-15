namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Represents a detected code violation.
/// </summary>
public sealed class Violation
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string FilePath { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
    public required string Severity { get; init; }
    public required string Category { get; init; }
    public required bool IsAutoFixable { get; init; }
}
