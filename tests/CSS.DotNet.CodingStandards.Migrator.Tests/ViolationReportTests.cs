namespace CSS.DotNet.CodingStandards.Migrator.Tests;

/// <summary>
/// Tests for the ViolationReport class.
/// </summary>
public sealed class ViolationReportTests
{
    [Fact]
    public void GetViolationsByStage_GroupsCorrectly()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "IDE0001", Title = "Test1", FilePath = "/test.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE0002", Title = "Test2", FilePath = "/test.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE1006", Title = "Test3", FilePath = "/test.cs", Line = 3, Column = 1, Severity = "Warning", Category = "Naming", IsAutoFixable = true },
        };

        var report = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 3,
            Violations = violations
        };

        // Act
        var byStage = report.GetViolationsByStage();

        // Assert
        Assert.Equal(2, byStage.Count);
        Assert.Equal(2, byStage["Formatting"].Count);
        Assert.Single(byStage["Naming"]);
    }

    [Fact]
    public void GetAutoFixableCount_ReturnsCorrectCount()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "IDE0001", Title = "Test1", FilePath = "/test.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "CA1000", Title = "Test2", FilePath = "/test.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Manual", IsAutoFixable = false },
            new() { Id = "IDE1006", Title = "Test3", FilePath = "/test.cs", Line = 3, Column = 1, Severity = "Warning", Category = "Naming", IsAutoFixable = true },
        };

        var report = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 3,
            Violations = violations
        };

        // Act
        var count = report.GetAutoFixableCount();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void GetViolationsByFile_GroupsCorrectly()
    {
        // Arrange
        var violations = new List<Violation>
        {
            new() { Id = "IDE0001", Title = "Test1", FilePath = "/test1.cs", Line = 1, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE0002", Title = "Test2", FilePath = "/test1.cs", Line = 2, Column = 1, Severity = "Warning", Category = "Formatting", IsAutoFixable = true },
            new() { Id = "IDE1006", Title = "Test3", FilePath = "/test2.cs", Line = 3, Column = 1, Severity = "Warning", Category = "Naming", IsAutoFixable = true },
        };

        var report = new ViolationReport
        {
            SolutionPath = "/test.sln",
            TotalViolations = 3,
            Violations = violations
        };

        // Act
        var byFile = report.GetViolationsByFile();

        // Assert
        Assert.Equal(2, byFile.Count);
        Assert.Equal(2, byFile["/test1.cs"].Count);
        Assert.Single(byFile["/test2.cs"]);
    }
}
