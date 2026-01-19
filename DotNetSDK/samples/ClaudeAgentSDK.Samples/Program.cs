using System.Text.Json;
using ClaudeAgentSDK;
using ClaudeAgentSDK.Models;

// ============================================
// DIAGNOSTIC SAMPLE - Verbose logging enabled
// ============================================

var workingDir = Environment.CurrentDirectory;
var logFile = Path.Combine(workingDir, "claude-sdk-debug.log");

// Simple logging helper
void Log(string message, bool alsoConsole = true)
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    var line = $"[{timestamp}] {message}";
    File.AppendAllText(logFile, line + Environment.NewLine);
    if (alsoConsole)
    {
        Console.WriteLine(line);
    }
}

// Clear previous log
if (File.Exists(logFile))
{
    File.Delete(logFile);
}

Log($"=== Claude Agent SDK Diagnostic Sample ===");
Log($"Working directory: {workingDir}");
Log($"Log file: {logFile}");
Log($"");

// Stderr callback to capture CLI debug output
void OnStderr(string line)
{
    Log($"[STDERR] {line}");
}

// ============================================
// Test 1: Simple query with verbose output
// ============================================
Log("=== Test 1: Simple One-Shot Query ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        StderrCallback = OnStderr
    };

    Log("Starting query: 'What is 2 + 2?'");

    int messageCount = 0;
    await foreach (var message in ClaudeAgent.QueryAsync("What is 2 + 2? Reply briefly.", options))
    {
        messageCount++;
        Log($"Received message #{messageCount}: Type={message.Type}");

        switch (message)
        {
            case AssistantMessage assistant:
                Log($"  Model: {assistant.Model}");
                foreach (var block in assistant.Content)
                {
                    if (block is TextBlock text)
                    {
                        Log($"  Text: {text.Text}");
                    }
                    else if (block is ToolUseBlock tool)
                    {
                        Log($"  Tool: {tool.Name} (id={tool.Id})");
                    }
                }
                break;

            case UserMessage user:
                var content = user.GetContentAsString();
                if (content != null)
                {
                    Log($"  Content: {content}");
                }
                else
                {
                    Log($"  Content blocks: {user.GetContentBlocks()?.Count ?? 0}");
                }
                break;

            case SystemMessage system:
                Log($"  Subtype: {system.Subtype}");
                break;

            case ResultMessage result:
                Log($"  Result: SessionId={result.SessionId}, Turns={result.NumTurns}, Duration={result.DurationMs}ms");
                Log($"  IsError={result.IsError}, Cost=${result.TotalCostUsd:F4}");
                if (!string.IsNullOrEmpty(result.Result))
                {
                    Log($"  Result text: {result.Result}");
                }
                break;

            default:
                Log($"  (Unknown message type: {message.GetType().Name})");
                break;
        }
    }

    Log($"Test 1 complete. Total messages received: {messageCount}");
}
catch (Exception ex)
{
    Log($"Test 1 FAILED: {ex.GetType().Name}: {ex.Message}");
    Log($"Stack trace: {ex.StackTrace}", alsoConsole: false);
}

Log("");

// ============================================
// Test 2: Check if CLI is reachable
// ============================================
Log("=== Test 2: CLI Reachability Check ===");
try
{
    // Try to find the CLI
    var cliPath = FindCli();
    Log($"CLI found at: {cliPath}");

    // Try running --version
    var process = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = cliPath,
            Arguments = "--version",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    process.Start();
    var stdout = await process.StandardOutput.ReadToEndAsync();
    var stderr = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    Log($"CLI version output: {stdout.Trim()}");
    if (!string.IsNullOrWhiteSpace(stderr))
    {
        Log($"CLI stderr: {stderr.Trim()}");
    }
    Log($"CLI exit code: {process.ExitCode}");
}
catch (Exception ex)
{
    Log($"Test 2 FAILED: {ex.GetType().Name}: {ex.Message}");
}

Log("");

// ============================================
// Test 3: Interactive client (streaming mode)
// ============================================
Log("=== Test 3: Interactive Client (Streaming Mode) ===");
try
{
    var clientOptions = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        StderrCallback = OnStderr
    };

    Log("Creating client...");
    await using var client = ClaudeAgent.CreateClient(clientOptions);

    Log("Connecting with prompt: 'Say hello briefly'");
    await client.ConnectAsync("Say hello briefly");

    Log("Receiving response...");
    int msgCount = 0;
    await foreach (var message in client.ReceiveResponseAsync())
    {
        msgCount++;
        Log($"Received message #{msgCount}: Type={message.Type}");

        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Log($"  Text: {text.Text}");
                }
            }
        }
        else if (message is ResultMessage result)
        {
            Log($"  Result: {result.SessionId}, {result.NumTurns} turns");
        }
    }

    Log($"Test 3 complete. Messages received: {msgCount}");
}
catch (Exception ex)
{
    Log($"Test 3 FAILED: {ex.GetType().Name}: {ex.Message}");
    Log($"Stack trace:", alsoConsole: false);
    Log(ex.StackTrace ?? "", alsoConsole: false);
}

Log("");
Log("=== All tests complete ===");
Log($"See {logFile} for full debug output");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();


// Helper to find CLI (mirrors SDK logic)
static string FindCli()
{
    // Check PATH
    var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
    var exeName = OperatingSystem.IsWindows() ? "claude.exe" : "claude";

    foreach (var dir in pathDirs)
    {
        var fullPath = Path.Combine(dir, exeName);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }
    }

    // Check common locations
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var commonPaths = new[]
    {
        Path.Combine(home, ".npm-global", "bin", exeName),
        Path.Combine(home, "AppData", "Roaming", "npm", exeName),
        "/usr/local/bin/claude",
        Path.Combine(home, ".local", "bin", "claude"),
    };

    foreach (var path in commonPaths)
    {
        if (File.Exists(path))
        {
            return path;
        }
    }

    throw new FileNotFoundException($"Claude CLI not found. Searched PATH and common locations.");
}
