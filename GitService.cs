using System.Diagnostics;
using System.IO;

namespace KOYA_APP
{
    public static class GitService
    {
        public static async Task<(bool Success, string Message)> SyncConfigAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string? repoDir = FindRepoRoot(baseDir);
                    
                    if (repoDir == null)
                    {
                        return (false, "Could not find Git repository root (missing .git folder in hierarchy).");
                    }

                    // 1. git add config.json
                    // Musimy upewnić się, że ścieżka do config.json jest poprawna względem repoDir
                    string configPath = Path.Combine(baseDir, "config.json");
                    string relativeConfigPath = Path.GetRelativePath(repoDir, configPath);

                    var addResult = RunGitCommand($"add \"{relativeConfigPath}\"", repoDir);
                    if (!addResult.Success) return (false, "Git add failed: " + addResult.Output);

                    // 2. git commit -m "Update config from KOYA-APP"
                    var commitResult = RunGitCommand("commit -m \"Update config from KOYA-APP\"", repoDir);
                    if (!commitResult.Success && !commitResult.Output.Contains("nothing to commit"))
                        return (false, "Git commit failed: " + commitResult.Output);

                    // 3. git push
                    var pushResult = RunGitCommand("push", repoDir);
                    if (!pushResult.Success) return (false, "Git push failed: " + pushResult.Output);

                    return (true, "Configuration synced to GitHub successfully.");
                }
                catch (Exception ex)
                {
                    return (false, "Sync failed: " + ex.Message);
                }
            });
        }

        private static string? FindRepoRoot(string startDir)
        {
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        private static (bool Success, string Output) RunGitCommand(string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process == null) return (false, "Could not start git process.");

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                bool success = process.ExitCode == 0;
                return (success, success ? output : error);
            }
        }
    }
}
