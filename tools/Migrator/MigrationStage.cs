namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Migration stages in order of execution.
/// </summary>
public enum MigrationStage
{
    /// <summary>
    /// Code formatting issues (indentation, spacing, etc.)
    /// </summary>
    Formatting = 0,

    /// <summary>
    /// Naming convention issues
    /// </summary>
    Naming = 1,

    /// <summary>
    /// Logic and code quality issues that can be auto-fixed
    /// </summary>
    Logic = 2,

    /// <summary>
    /// Issues that require manual intervention
    /// </summary>
    Manual = 3
}
