using LibGit2Sharp;

namespace CSS.DotNet.CodingStandards.Migrator;

/// <summary>
/// Handles Git operations for staged commits and rollback.
/// </summary>
public sealed class GitManager
{
    private readonly string _repositoryPath;

    public GitManager(string repositoryPath)
    {
        this._repositoryPath = repositoryPath;
    }

    /// <summary>
    /// Creates a commit for a specific migration stage.
    /// </summary>
    public bool CommitStage(MigrationStage stage, string additionalMessage = "")
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);

            // Check if there are any changes
            var status = repo.RetrieveStatus();
            if (!status.IsDirty)
            {
                Console.WriteLine("No changes to commit");
                return false;
            }

            // Stage all changes
            Commands.Stage(repo, "*");

            // Create commit
            var signature = repo.Config.BuildSignature(DateTimeOffset.UtcNow);
            var message = $"Apply {stage} stage fixes from CSS.DotNet.CodingStandards migration";
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                message += $"\n\n{additionalMessage}";
            }

            var commit = repo.Commit(message, signature, signature);
            Console.WriteLine($"Created commit: {commit.Sha[..7]} - {message.Split('\n')[0]}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to commit: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Creates a tag for a migration checkpoint.
    /// </summary>
    public bool CreateCheckpoint(string checkpointName)
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);
            var signature = repo.Config.BuildSignature(DateTimeOffset.UtcNow);

            var tag = repo.Tags.Add($"migration-{checkpointName}", repo.Head.Tip, signature, $"Migration checkpoint: {checkpointName}");
            Console.WriteLine($"Created checkpoint tag: {tag.FriendlyName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create checkpoint: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Rolls back to a specific commit or tag.
    /// </summary>
    public bool Rollback(string commitOrTag)
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);

            // Find the commit
            var commit = repo.Lookup<Commit>(commitOrTag) ??
                         repo.Tags[commitOrTag]?.Target as Commit;

            if (commit == null)
            {
                Console.WriteLine($"Could not find commit or tag: {commitOrTag}");
                return false;
            }

            // Reset to the commit
            repo.Reset(ResetMode.Hard, commit);
            Console.WriteLine($"Rolled back to: {commit.Sha[..7]}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to rollback: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the current repository status.
    /// </summary>
    public string GetStatus()
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);
            var status = repo.RetrieveStatus();

            var statusText = new System.Text.StringBuilder();
            statusText.AppendLine($"Branch: {repo.Head.FriendlyName}");
            statusText.AppendLine($"Commit: {repo.Head.Tip.Sha[..7]} - {repo.Head.Tip.MessageShort}");
            statusText.AppendLine($"Modified files: {status.Modified.Count()}");
            statusText.AppendLine($"Added files: {status.Added.Count()}");
            statusText.AppendLine($"Removed files: {status.Removed.Count()}");

            return statusText.ToString();
        }
        catch (Exception ex)
        {
            return $"Failed to get status: {ex.Message}";
        }
    }

    /// <summary>
    /// Checks if the repository is in a clean state.
    /// </summary>
    public bool IsClean()
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);
            return !repo.RetrieveStatus().IsDirty;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lists migration checkpoints.
    /// </summary>
    public List<string> ListCheckpoints()
    {
        try
        {
            using var repo = new Repository(this._repositoryPath);
            return repo.Tags
                .Where(t => t.FriendlyName.StartsWith("migration-", StringComparison.Ordinal))
                .Select(t => t.FriendlyName)
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
