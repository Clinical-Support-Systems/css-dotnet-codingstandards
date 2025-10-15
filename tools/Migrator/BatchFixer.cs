using System.Diagnostics;

namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Applies automatic fixes to code violations in batches.
/// </summary>
public sealed class BatchFixer
{
    /// <summary>
    /// Applies fixes for a specific migration stage.
    /// </summary>
    public async Task<bool> ApplyFixesAsync(
        string solutionOrProjectPath,
        MigrationStage stage,
        bool dryRun,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report($"Starting {stage} stage fixes...");

        var diagnostics = GetDiagnosticsForStage(stage);
        if (diagnostics.Length == 0)
        {
            progress?.Report($"No diagnostics configured for {stage} stage");
            return false;
        }

        var diagnosticsArg = string.Join(" ", diagnostics);
        var arguments = $"format \"{solutionOrProjectPath}\" --diagnostics {diagnosticsArg} --verbosity diagnostic";

        if (dryRun)
        {
            arguments += " --verify-no-changes";
            progress?.Report("Running in dry-run mode (verify-no-changes)");
        }

        progress?.Report($"Executing: dotnet {arguments}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputBuilder.AppendLine(args.Data);
                progress?.Report(args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                errorBuilder.AppendLine(args.Data);
                progress?.Report($"ERROR: {args.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            progress?.Report($"dotnet format exited with code {process.ExitCode}");
            if (errorBuilder.Length > 0)
            {
                progress?.Report($"Errors:\n{errorBuilder}");
            }

            return false;
        }

        progress?.Report($"{stage} stage fixes completed successfully");
        return true;
    }

    /// <summary>
    /// Applies fixes for all stages in sequence.
    /// </summary>
    public async Task<Dictionary<MigrationStage, bool>> ApplyAllStagesAsync(
        string solutionOrProjectPath,
        bool dryRun,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<MigrationStage, bool>();

        foreach (var stage in Enum.GetValues<MigrationStage>())
        {
            if (stage == MigrationStage.Manual)
            {
                progress?.Report("Skipping Manual stage (requires human intervention)");
                results[stage] = false;
                continue;
            }

            progress?.Report($"\n{'='.ToString().PadRight(80, '=')}");
            progress?.Report($"Processing {stage} stage");
            progress?.Report($"{'='.ToString().PadRight(80, '=')}\n");

            var success = await this.ApplyFixesAsync(solutionOrProjectPath, stage, dryRun, progress, cancellationToken);
            results[stage] = success;

            if (!success && !dryRun)
            {
                progress?.Report($"Failed to apply {stage} stage fixes. Stopping.");
                break;
            }
        }

        return results;
    }

    private static string[] GetDiagnosticsForStage(MigrationStage stage)
    {
        return stage switch
        {
            MigrationStage.Formatting => [
                "IDE0001", "IDE0004", "IDE0005", "IDE0007", "IDE0009", "IDE0011",
                "IDE0055", "IDE0161", "IDE2000", "IDE2001", "IDE2002", "IDE2003",
                "IDE2004", "IDE2005", "IDE2006"
            ],
            MigrationStage.Naming => [
                "IDE1006"
            ],
            MigrationStage.Logic => [
                // Auto-fixable logic rules
                "IDE0010", "IDE0017", "IDE0018", "IDE0019", "IDE0020", "IDE0021",
                "IDE0022", "IDE0023", "IDE0024", "IDE0025", "IDE0026", "IDE0027",
                "IDE0028", "IDE0029", "IDE0030", "IDE0031", "IDE0032", "IDE0033",
                "IDE0034", "IDE0035", "IDE0036", "IDE0037", "IDE0039", "IDE0040",
                "IDE0041", "IDE0042", "IDE0044", "IDE0045", "IDE0046", "IDE0047",
                "IDE0048", "IDE0049", "IDE0050", "IDE0051", "IDE0052", "IDE0053",
                "IDE0054", "IDE0056", "IDE0057", "IDE0058", "IDE0059", "IDE0060",
                "IDE0061", "IDE0062", "IDE0063", "IDE0064", "IDE0065", "IDE0066",
                "IDE0070", "IDE0071", "IDE0072", "IDE0073", "IDE0074", "IDE0075",
                "IDE0076", "IDE0077", "IDE0078", "IDE0080", "IDE0081", "IDE0082",
                "IDE0083", "IDE0084", "IDE0090", "IDE0100", "IDE0110", "IDE0120",
                "IDE0130", "IDE0150", "IDE0160", "IDE0170", "IDE0180", "IDE0200",
                "IDE0210", "IDE0220", "IDE0230", "IDE0240", "IDE0241", "IDE0250",
                "IDE0251", "IDE0260", "IDE0270", "IDE0280", "IDE0290", "IDE0300",
                "IDE0305", "IDE1005"
            ],
            MigrationStage.Manual => [],
            _ => []
        };
    }
}
