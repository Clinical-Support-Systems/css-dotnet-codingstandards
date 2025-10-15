namespace CSS.DotNet.CodingStandards.Migrator.Tests;

/// <summary>
/// Tests for the ReportGenerator class.
/// </summary>
public sealed class ReportGeneratorTests
{
    [Fact]
    public void GenerateTextReport_CreatesFormattedReport()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "IDE0001", Title = "Simplify name", FilePath = "/test.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE1006", Title = "Naming rule", FilePath = "/test.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Naming", IsAutoFixable = true },
        };

        var report = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 2,
            Violations = violations
        };

        var generator = new ReportGenerator();

        // Act
        var textReport = generator.GenerateTextReport(report);

        // Assert
        Assert.Contains("CSS.DotNet.CodingStandards Migration Report", textReport);
        Assert.Contains("Total Violations: 2", textReport);
        Assert.Contains("Auto-fixable: 2", textReport);
        Assert.Contains("Formatting", textReport);
        Assert.Contains("Naming", textReport);
    }

    [Fact]
    public void GeneratePRDescription_CreatesFormattedDescription()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "IDE0001", Title = "Simplify name", FilePath = "/test.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE0002", Title = "Simplify name", FilePath = "/test.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
        };

        var beforeReport = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 2,
            Violations = violations
        };

        var afterReport = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 0,
            Violations = []
        };

        var generator = new ReportGenerator();

        // Act
        var prDescription = generator.GeneratePRDescription(beforeReport, afterReport, MigrationStage.Formatting);

        // Assert
        Assert.Contains("## Migration: Formatting Stage", prDescription);
        Assert.Contains("Fixes code formatting issues", prDescription);
        Assert.Contains("| Metric | Before | After | Change |", prDescription);
    }

    [Fact]
    public void GenerateSuppressionSuggestions_CreatesValidSuggestions()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "CA1000", Title = "Manual rule", FilePath = "/src/test.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Manual", IsAutoFixable = false },
            new() { Id = "CA1000", Title = "Manual rule", FilePath = "/src/test2.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Manual", IsAutoFixable = false },
        };

        var report = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 2,
            Violations = violations
        };

        var generator = new ReportGenerator();

        // Act
        var suggestions = generator.GenerateSuppressionSuggestions(report);

        // Assert
        Assert.Contains("Suggested Suppressions for Problematic Areas", suggestions);
        Assert.Contains("CA1000", suggestions);
        Assert.Contains("dotnet_diagnostic.CA1000.severity = none", suggestions);
    }
}
