using System.CommandLine;
using CSS.DotNet.CodingStandards.Migrator;

var rootCommand = new RootCommand("CSS.DotNet.CodingStandards Migration Assistant")
{
    Description = "Automated migration tool for adopting CSS.DotNet.CodingStandards package"
};

// Analyze command
var analyzeCommand = new Command("analyze", "Analyze a codebase and generate a violation report");
var analyzePathArg = new Argument<string>("path", "Path to solution or project file");
var outputOption = new Option<string?>("--output", "Output file for the report (optional)");
var formatOption = new Option<string>("--format", () => "text", "Report format: text, json");

analyzeCommand.AddArgument(analyzePathArg);
analyzeCommand.AddOption(outputOption);
analyzeCommand.AddOption(formatOption);

analyzeCommand.SetHandler(async (string path, string? output, string format) =>
{
    await AnalyzeCommandAsync(path, output, format);
}, analyzePathArg, outputOption, formatOption);

// Fix command
var fixCommand = new Command("fix", "Apply automatic fixes for violations");
var fixPathArg = new Argument<string>("path", "Path to solution or project file");
var stageOption = new Option<string?>("--stage", "Specific stage to fix: formatting, naming, logic, or all (default)");
var dryRunOption = new Option<bool>("--dry-run", "Perform a dry run without making changes");
var commitOption = new Option<bool>("--commit", "Automatically commit changes after each stage");

fixCommand.AddArgument(fixPathArg);
fixCommand.AddOption(stageOption);
fixCommand.AddOption(dryRunOption);
fixCommand.AddOption(commitOption);

fixCommand.SetHandler(async (string path, string? stage, bool dryRun, bool commit) =>
{
    await FixCommandAsync(path, stage, dryRun, commit);
}, fixPathArg, stageOption, dryRunOption, commitOption);

// Suggest command
var suggestCommand = new Command("suggest", "Generate suppression suggestions for problematic areas");
var suggestPathArg = new Argument<string>("path", "Path to solution or project file");
var suggestOutputOption = new Option<string?>("--output", "Output file for suggestions (optional)");

suggestCommand.AddArgument(suggestPathArg);
suggestCommand.AddOption(suggestOutputOption);

suggestCommand.SetHandler(async (string path, string? output) =>
{
    await SuggestCommandAsync(path, output);
}, suggestPathArg, suggestOutputOption);

// Rollback command
var rollbackCommand = new Command("rollback", "Rollback to a previous migration checkpoint");
var rollbackPathArg = new Argument<string>("path", "Path to repository");
var checkpointArg = new Argument<string>("checkpoint", "Checkpoint name or commit SHA");

rollbackCommand.AddArgument(rollbackPathArg);
rollbackCommand.AddArgument(checkpointArg);

rollbackCommand.SetHandler((string path, string checkpoint) =>
{
    RollbackCommand(path, checkpoint);
}, rollbackPathArg, checkpointArg);

// Status command
var statusCommand = new Command("status", "Show current repository and migration status");
var statusPathArg = new Argument<string>("path", "Path to repository");

statusCommand.AddArgument(statusPathArg);

statusCommand.SetHandler((string path) =>
{
    StatusCommand(path);
}, statusPathArg);

rootCommand.AddCommand(analyzeCommand);
rootCommand.AddCommand(fixCommand);
rootCommand.AddCommand(suggestCommand);
rootCommand.AddCommand(rollbackCommand);
rootCommand.AddCommand(statusCommand);

return await rootCommand.InvokeAsync(args);

static async Task AnalyzeCommandAsync(string path, string? output, string format)
{
    try
    {
        if (!File.Exists(path))
        {
            await Console.Error.WriteLineAsync($"Error: File not found: {path}");
            return;
        }

        Console.WriteLine("Starting analysis...");
        var scanner = new ViolationScanner();
        var report = await scanner.ScanAsync(path);

        var generator = new ReportGenerator();
        var reportText = generator.GenerateTextReport(report);

        if (!string.IsNullOrEmpty(output))
        {
            await File.WriteAllTextAsync(output, reportText);
            await Console.Out.WriteLineAsync($"Report saved to: {output}");
        }
        else
        {
            await Console.Out.WriteLineAsync(reportText);
        }
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Error during analysis: {ex.Message}");
        await Console.Error.WriteLineAsync(ex.StackTrace);
    }
}

static async Task FixCommandAsync(string path, string? stage, bool dryRun, bool commit)
{
    try
    {
        if (!File.Exists(path))
        {
            await Console.Error.WriteLineAsync($"Error: File not found: {path}");
            return;
        }

        var repoPath = FindRepositoryRoot(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        GitManager? gitManager = null;

        if (commit && !string.IsNullOrEmpty(repoPath))
        {
            gitManager = new GitManager(repoPath);
            if (!gitManager.IsClean())
            {
                Console.WriteLine("Warning: Repository has uncommitted changes.");
                Console.Write("Continue? (y/n): ");
                var response = Console.ReadLine();
                if (response?.ToLowerInvariant() != "y")
                {
                    Console.WriteLine("Aborted.");
                    return;
                }
            }
        }

        var fixer = new BatchFixer();
        var progress = new Progress<string>(msg => Console.WriteLine(msg));

        if (stage != null && !stage.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<MigrationStage>(stage, ignoreCase: true, out var migrationStage))
            {
                await Console.Error.WriteLineAsync($"Error: Invalid stage '{stage}'. Valid values: formatting, naming, logic, all");
                return;
            }

            var success = await fixer.ApplyFixesAsync(path, migrationStage, dryRun, progress);

            if (success && commit && gitManager != null && !dryRun)
            {
                gitManager.CommitStage(migrationStage);
                gitManager.CreateCheckpoint($"{migrationStage}-{DateTime.UtcNow:yyyyMMddHHmmss}");
            }
        }
        else
        {
            var results = await fixer.ApplyAllStagesAsync(path, dryRun, progress);

            if (commit && gitManager != null && !dryRun)
            {
                foreach (var (stageResult, success) in results)
                {
                    if (success && stageResult != MigrationStage.Manual)
                    {
                        gitManager.CommitStage(stageResult);
                    }
                }

                gitManager.CreateCheckpoint($"all-stages-{DateTime.UtcNow:yyyyMMddHHmmss}");
            }
        }

        await Console.Out.WriteLineAsync("\nFix operation completed.");
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Error during fix: {ex.Message}");
        await Console.Error.WriteLineAsync(ex.StackTrace);
    }
}

static async Task SuggestCommandAsync(string path, string? output)
{
    try
    {
        if (!File.Exists(path))
        {
            await Console.Error.WriteLineAsync($"Error: File not found: {path}");
            return;
        }

        Console.WriteLine("Analyzing for suppression suggestions...");
        var scanner = new ViolationScanner();
        var report = await scanner.ScanAsync(path);

        var generator = new ReportGenerator();
        var suggestions = generator.GenerateSuppressionSuggestions(report);

        if (!string.IsNullOrEmpty(output))
        {
            await File.WriteAllTextAsync(output, suggestions);
            await Console.Out.WriteLineAsync($"Suggestions saved to: {output}");
        }
        else
        {
            await Console.Out.WriteLineAsync(suggestions);
        }
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Error generating suggestions: {ex.Message}");
        await Console.Error.WriteLineAsync(ex.StackTrace);
    }
}

static void RollbackCommand(string path, string checkpoint)
{
    try
    {
        var repoPath = FindRepositoryRoot(path);
        if (string.IsNullOrEmpty(repoPath))
        {
            Console.Error.WriteLine("Error: Not a git repository");
            return;
        }

        var gitManager = new GitManager(repoPath);

        Console.WriteLine($"Rolling back to checkpoint: {checkpoint}");
        Console.WriteLine("Warning: This will discard all uncommitted changes.");
        Console.Write("Continue? (y/n): ");
        var response = Console.ReadLine();

        if (response?.ToLowerInvariant() != "y")
        {
            Console.WriteLine("Aborted.");
            return;
        }

        if (gitManager.Rollback(checkpoint))
        {
            Console.WriteLine("Rollback completed successfully.");
        }
        else
        {
            Console.Error.WriteLine("Rollback failed.");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error during rollback: {ex.Message}");
    }
}

static void StatusCommand(string path)
{
    try
    {
        var repoPath = FindRepositoryRoot(path);
        if (string.IsNullOrEmpty(repoPath))
        {
            Console.Error.WriteLine("Error: Not a git repository");
            return;
        }

        var gitManager = new GitManager(repoPath);
        Console.WriteLine(gitManager.GetStatus());

        var checkpoints = gitManager.ListCheckpoints();
        if (checkpoints.Count > 0)
        {
            Console.WriteLine("\nMigration Checkpoints:");
            foreach (var checkpoint in checkpoints)
            {
                Console.WriteLine($"  - {checkpoint}");
            }
        }
        else
        {
            Console.WriteLine("\nNo migration checkpoints found.");
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error getting status: {ex.Message}");
    }
}

static string? FindRepositoryRoot(string startPath)
{
    var current = new DirectoryInfo(startPath);
    while (current != null)
    {
        if (Directory.Exists(Path.Combine(current.FullName, ".git")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    return null;
}
