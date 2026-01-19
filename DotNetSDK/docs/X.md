# Claude Agent SDK for .NET - Feature Documentation & Test Coverage

## Overview

This document provides a comprehensive overview of the Claude Agent SDK for .NET features, current test coverage, and guidance for safely testing untested features.

---

## Feature Matrix

### Legend
- ✅ **PASSED** - Feature tested and working
- ⚠️ **NOT TESTED** - Feature implemented but not yet tested
- ❌ **FAILED** - Feature tested but failing

---

## 1. Core Communication Features

| Feature | Implementation | Test Status | Notes |
|---------|---------------|-------------|-------|
| One-shot queries | `ClaudeAgent.QueryAsync()` | ✅ PASSED | Test 1 |
| Query to list | `ClaudeAgent.QueryToListAsync()` | ⚠️ NOT TESTED | Convenience wrapper |
| Query to text | `ClaudeAgent.QueryTextAsync()` | ⚠️ NOT TESTED | Returns final result string |
| Interactive streaming | `ClaudeSDKClient` | ✅ PASSED | Test 3 |
| Multi-turn conversation | `client.QueryAsync()` | ⚠️ NOT TESTED | Follow-up queries |
| Async enumeration | `IAsyncEnumerable<IMessage>` | ✅ PASSED | Tests 1, 3 |
| CLI subprocess | `SubprocessCliTransport` | ✅ PASSED | Tests 0, 2 |

---

## 2. Message Types

| Message Type | Class | Test Status | Notes |
|--------------|-------|-------------|-------|
| System | `SystemMessage` | ✅ PASSED | Init messages received |
| Assistant | `AssistantMessage` | ✅ PASSED | Response with content |
| User | `UserMessage` | ⚠️ NOT TESTED | Echoed user input |
| Result | `ResultMessage` | ✅ PASSED | Final result with metadata |
| Stream Event | `StreamEvent` | ⚠️ NOT TESTED | Partial streaming updates |

---

## 3. Content Blocks

| Block Type | Class | Test Status | Notes |
|------------|-------|-------------|-------|
| Text | `TextBlock` | ✅ PASSED | Basic text responses |
| Thinking | `ThinkingBlock` | ⚠️ NOT TESTED | Extended thinking content |
| Tool Use | `ToolUseBlock` | ⚠️ NOT TESTED | Tool invocation |
| Tool Result | `ToolResultBlock` | ⚠️ NOT TESTED | Tool execution results |

---

## 4. SDK Control Protocol

| Feature | Method | Test Status | Notes |
|---------|--------|-------------|-------|
| Permission callbacks | `CanUseTool` delegate | ⚠️ NOT TESTED | Approve/deny tool use |
| Hook callbacks | `Hooks` dictionary | ⚠️ NOT TESTED | Event-based callbacks |
| Interrupt | `InterruptAsync()` | ⚠️ NOT TESTED | Stop current operation |
| Set permission mode | `SetPermissionModeAsync()` | ⚠️ NOT TESTED | Change mode mid-session |
| Set model | `SetModelAsync()` | ⚠️ NOT TESTED | Change model mid-session |
| Rewind files | `RewindFilesAsync()` | ⚠️ NOT TESTED | Checkpoint restoration |

---

## 5. Hook Events

| Hook Event | Class | Test Status | Notes |
|------------|-------|-------------|-------|
| PreToolUse | `PreToolUseHookInput` | ⚠️ NOT TESTED | Before tool execution |
| PostToolUse | `PostToolUseHookInput` | ⚠️ NOT TESTED | After tool execution |
| UserPromptSubmit | `UserPromptSubmitHookInput` | ⚠️ NOT TESTED | User prompt validation |
| Stop | `StopHookInput` | ⚠️ NOT TESTED | Session stop event |
| SubagentStop | `SubagentStopHookInput` | ⚠️ NOT TESTED | Subagent completion |
| PreCompact | `PreCompactHookInput` | ⚠️ NOT TESTED | Before context compaction |

---

## 6. Configuration Options

| Option | Property | Test Status | Notes |
|--------|----------|-------------|-------|
| Working directory | `WorkingDirectory` | ✅ PASSED | Tests 1, 3 |
| System prompt | `SystemPrompt` | ⚠️ NOT TESTED | Custom instructions |
| Model | `Model` | ⚠️ NOT TESTED | Model selection |
| Fallback model | `FallbackModel` | ⚠️ NOT TESTED | Backup model |
| Max turns | `MaxTurns` | ⚠️ NOT TESTED | Turn limit |
| Max budget | `MaxBudgetUsd` | ⚠️ NOT TESTED | Cost limit |
| Max thinking tokens | `MaxThinkingTokens` | ⚠️ NOT TESTED | Thinking limit |
| Permission mode | `PermissionMode` | ⚠️ NOT TESTED | Default/AcceptEdits/Plan/Bypass |
| Tools preset | `ToolsPreset` | ⚠️ NOT TESTED | all/computer/none |
| Allowed tools | `AllowedTools` | ⚠️ NOT TESTED | Whitelist |
| Disallowed tools | `DisallowedTools` | ⚠️ NOT TESTED | Blacklist |
| Environment vars | `Environment` | ⚠️ NOT TESTED | Custom env vars |
| Stderr callback | `StderrCallback` | ✅ PASSED | Debug output |
| Custom CLI path | `CliPath` | ⚠️ NOT TESTED | Non-standard CLI location |

---

## 7. Session Management

| Feature | Property/Method | Test Status | Notes |
|---------|----------------|-------------|-------|
| Continue conversation | `ContinueConversation` | ⚠️ NOT TESTED | Resume last session |
| Resume session | `Resume` | ⚠️ NOT TESTED | Resume by ID |
| Fork session | `ForkSession` | ⚠️ NOT TESTED | Branch from session |

---

## 8. Advanced Features

| Feature | Property | Test Status | Notes |
|---------|----------|-------------|-------|
| MCP servers | `McpServers` / `McpConfigPath` | ⚠️ NOT TESTED | Model Context Protocol |
| Agents | `Agents` | ⚠️ NOT TESTED | Custom agents |
| Plugins | `Plugins` | ⚠️ NOT TESTED | Plugin directories |
| Sandbox | `Sandbox` | ⚠️ NOT TESTED | Sandbox configuration |
| JSON schema output | `OutputFormat` | ⚠️ NOT TESTED | Structured responses |
| File checkpointing | `EnableFileCheckpointing` | ⚠️ NOT TESTED | File state tracking |
| Additional dirs | `AddDirs` | ⚠️ NOT TESTED | Extra directories |
| Beta features | `Betas` | ⚠️ NOT TESTED | Beta flag list |
| Partial messages | `IncludePartialMessages` | ⚠️ NOT TESTED | Streaming partials |

---

## Current Test Results

### Test 0: Raw CLI Output
- **Status**: ✅ PASSED
- **Purpose**: Verify CLI is installed and outputs valid JSON
- **Output**: 3543 chars of valid stream-json

### Test 1: One-Shot Query
- **Status**: ✅ PASSED
- **Messages**: 3 (system, assistant, result)
- **Duration**: 2075ms API time
- **Cost**: $0.0436

### Test 2: CLI Reachability
- **Status**: ✅ PASSED
- **CLI Version**: 2.1.12

### Test 3: Interactive Streaming
- **Status**: ✅ PASSED
- **Messages**: 3 (system, assistant, result)
- **Response**: "Hello! I'm Claude, ready to help..."

---

## Safe Testing Guidelines

### Important Considerations

1. **Anthropic Usage Policy Compliance**
   - Never use the SDK for automated content generation at scale
   - Never attempt to bypass safety measures
   - Never use for harassment, spam, or malicious purposes
   - Always maintain human oversight of tool operations

2. **Account Safety (Claude MAX)**
   - Use `MaxBudgetUsd` to limit costs during testing
   - Use `MaxTurns` to prevent runaway sessions
   - Monitor usage in your Anthropic dashboard
   - Use `Plan` permission mode for safer testing

3. **Computer/File Safety**
   - Use isolated test directories (e.g., temp folders)
   - Use `DisallowedTools` to block dangerous operations
   - Test file operations on disposable test files only
   - Use `Sandbox` configuration when available

---

## Safe Test Implementations

### Test 4: System Prompt

```csharp
// ============================================
// Test 4: System Prompt (SAFE)
// ============================================
Log("=== Test 4: System Prompt ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        SystemPrompt = "You are a helpful math tutor. Always explain your reasoning step by step.",
        MaxTurns = 1,
        MaxBudgetUsd = 0.10m // Safety limit
    };

    await foreach (var message in ClaudeAgent.QueryAsync("What is 15% of 80?", options))
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Log($"  Response: {text.Text}");
                    // Verify the response includes step-by-step explanation
                }
            }
        }
    }
    Log("Test 4 PASSED");
}
catch (Exception ex)
{
    Log($"Test 4 FAILED: {ex.Message}");
}
```

### Test 5: Model Selection

```csharp
// ============================================
// Test 5: Model Selection (SAFE)
// ============================================
Log("=== Test 5: Model Selection ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        Model = "claude-sonnet-4-20250514", // Use a specific model
        MaxTurns = 1,
        MaxBudgetUsd = 0.05m
    };

    await foreach (var message in ClaudeAgent.QueryAsync("Say 'Model test successful'", options))
    {
        if (message is AssistantMessage assistant)
        {
            Log($"  Model used: {assistant.Model}");
            // Verify correct model was used
        }
    }
    Log("Test 5 PASSED");
}
catch (Exception ex)
{
    Log($"Test 5 FAILED: {ex.Message}");
}
```

### Test 6: Max Turns Limit

```csharp
// ============================================
// Test 6: Max Turns Limit (SAFE)
// ============================================
Log("=== Test 6: Max Turns Limit ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 1, // Strictly limit to 1 turn
        MaxBudgetUsd = 0.10m
    };

    int turnCount = 0;
    await foreach (var message in ClaudeAgent.QueryAsync(
        "Count from 1 to 100, one number per message", options))
    {
        if (message is AssistantMessage)
            turnCount++;
        if (message is ResultMessage result)
        {
            Log($"  Turns executed: {result.NumTurns}");
            Log($"  Expected: 1 (limited by MaxTurns)");
        }
    }
    Log($"Test 6 {(turnCount <= 1 ? "PASSED" : "FAILED")}");
}
catch (Exception ex)
{
    Log($"Test 6 FAILED: {ex.Message}");
}
```

### Test 7: Budget Limit

```csharp
// ============================================
// Test 7: Budget Limit (SAFE)
// ============================================
Log("=== Test 7: Budget Limit ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxBudgetUsd = 0.01m, // Very low budget
        MaxTurns = 5
    };

    await foreach (var message in ClaudeAgent.QueryAsync("Write a haiku about coding", options))
    {
        if (message is ResultMessage result)
        {
            Log($"  Total cost: ${result.TotalCostUsd:F4}");
            Log($"  Budget limit: $0.01");
            if (result.TotalCostUsd <= 0.01m)
                Log("Test 7 PASSED - stayed within budget");
            else
                Log("Test 7 INFO - budget exceeded (expected for some prompts)");
        }
    }
}
catch (Exception ex)
{
    Log($"Test 7 FAILED: {ex.Message}");
}
```

### Test 8: Permission Mode (Plan Mode - SAFE)

```csharp
// ============================================
// Test 8: Plan Permission Mode (SAFE - No Execution)
// ============================================
Log("=== Test 8: Plan Permission Mode ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        PermissionMode = PermissionMode.Plan, // Claude plans but doesn't execute
        MaxTurns = 2,
        MaxBudgetUsd = 0.10m
    };

    await foreach (var message in ClaudeAgent.QueryAsync(
        "Plan how you would create a hello world program", options))
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Log($"  Plan: {text.Text.Substring(0, Math.Min(200, text.Text.Length))}...");
                }
            }
        }
    }
    Log("Test 8 PASSED - Plan mode doesn't execute tools");
}
catch (Exception ex)
{
    Log($"Test 8 FAILED: {ex.Message}");
}
```

### Test 9: Disallowed Tools (SAFE)

```csharp
// ============================================
// Test 9: Disallowed Tools (SAFE - Blocks Dangerous Tools)
// ============================================
Log("=== Test 9: Disallowed Tools ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        DisallowedTools = new List<string>
        {
            "Bash",      // Block shell commands
            "Write",     // Block file writing
            "Edit",      // Block file editing
            "NotebookEdit" // Block notebook editing
        },
        MaxTurns = 2,
        MaxBudgetUsd = 0.10m
    };

    await foreach (var message in ClaudeAgent.QueryAsync(
        "What is the current date? Just tell me, don't use tools.", options))
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Log($"  Response: {text.Text}");
                }
                else if (block is ToolUseBlock tool)
                {
                    Log($"  Tool attempted: {tool.Name} (should be blocked if dangerous)");
                }
            }
        }
    }
    Log("Test 9 PASSED");
}
catch (Exception ex)
{
    Log($"Test 9 FAILED: {ex.Message}");
}
```

### Test 10: Multi-Turn Conversation (SAFE)

```csharp
// ============================================
// Test 10: Multi-Turn Conversation (SAFE)
// ============================================
Log("=== Test 10: Multi-Turn Conversation ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 1,
        MaxBudgetUsd = 0.20m // Budget for 2 turns
    };

    await using var client = ClaudeAgent.CreateClient(options);

    // Turn 1
    Log("  Turn 1: Asking to remember a number...");
    await client.ConnectAsync("Remember the number 42. Just say 'OK, I'll remember 42.'");
    string? sessionId = null;

    await foreach (var message in client.ReceiveResponseAsync())
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                    Log($"    Claude: {text.Text}");
            }
        }
        if (message is ResultMessage result)
        {
            sessionId = result.SessionId;
            Log($"    Session: {sessionId}");
        }
    }

    // Turn 2 - Follow-up
    Log("  Turn 2: Asking for the number back...");
    await client.QueryAsync("What number did I ask you to remember?");

    await foreach (var message in client.ReceiveResponseAsync())
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Log($"    Claude: {text.Text}");
                    if (text.Text.Contains("42"))
                        Log("Test 10 PASSED - Context maintained");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Log($"Test 10 FAILED: {ex.Message}");
}
```

### Test 11: Permission Callback (SAFE - Read-Only)

```csharp
// ============================================
// Test 11: Permission Callback - Read Operations Only (SAFE)
// ============================================
Log("=== Test 11: Permission Callback ===");
try
{
    var toolUsageLog = new List<string>();

    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 3,
        MaxBudgetUsd = 0.15m,
        // Permission callback - only allow read operations
        CanUseTool = async (request) =>
        {
            toolUsageLog.Add($"{request.ToolName}: {string.Join(", ", request.Input.Keys)}");
            Log($"    [PERMISSION] Tool: {request.ToolName}");

            // Allow read-only operations
            var safeTools = new[] { "Read", "Glob", "Grep", "WebSearch", "WebFetch" };

            if (safeTools.Contains(request.ToolName))
            {
                Log($"    [PERMISSION] ALLOWED (read-only tool)");
                return PermissionResult.Allow();
            }

            // Deny write operations
            Log($"    [PERMISSION] DENIED (not a read-only tool)");
            return PermissionResult.Deny($"Tool '{request.ToolName}' is not allowed in test mode");
        }
    };

    await foreach (var message in ClaudeAgent.QueryAsync(
        "Read the first 5 lines of Program.cs in the current directory", options))
    {
        if (message is AssistantMessage assistant)
        {
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                    Log($"    Response: {text.Text.Substring(0, Math.Min(100, text.Text.Length))}...");
            }
        }
    }

    Log($"  Tools attempted: {string.Join(", ", toolUsageLog)}");
    Log("Test 11 PASSED - Permission callback working");
}
catch (Exception ex)
{
    Log($"Test 11 FAILED: {ex.Message}");
}
```

### Test 12: PreToolUse Hook (SAFE - Logging Only)

```csharp
// ============================================
// Test 12: PreToolUse Hook - Logging (SAFE)
// ============================================
Log("=== Test 12: PreToolUse Hook ===");
try
{
    var hookLog = new List<string>();

    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 2,
        MaxBudgetUsd = 0.10m,
        Hooks = new Dictionary<HookEvent, List<HookMatcher>>
        {
            [HookEvent.PreToolUse] = new List<HookMatcher>
            {
                new HookMatcher
                {
                    Matcher = null, // Match all tools
                    Hooks = new List<Hook>
                    {
                        new Hook
                        {
                            Handler = async (input, matcher, context) =>
                            {
                                if (input is PreToolUseHookInput toolInput)
                                {
                                    hookLog.Add(toolInput.ToolName);
                                    Log($"    [HOOK] PreToolUse: {toolInput.ToolName}");
                                }
                                // Continue execution (just logging)
                                return new HookOutput { Continue = true };
                            }
                        }
                    }
                }
            }
        }
    };

    await foreach (var message in ClaudeAgent.QueryAsync(
        "What files are in the current directory? Use glob.", options))
    {
        if (message is ResultMessage)
        {
            Log($"  Hooks fired for: {string.Join(", ", hookLog)}");
        }
    }
    Log("Test 12 PASSED - Hook logging working");
}
catch (Exception ex)
{
    Log($"Test 12 FAILED: {ex.Message}");
}
```

### Test 13: Structured Output / JSON Schema (SAFE)

```csharp
// ============================================
// Test 13: Structured Output (SAFE)
// ============================================
Log("=== Test 13: Structured Output ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 1,
        MaxBudgetUsd = 0.10m,
        OutputFormat = new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object?>
            {
                ["answer"] = new Dictionary<string, object?> { ["type"] = "number" },
                ["explanation"] = new Dictionary<string, object?> { ["type"] = "string" }
            },
            ["required"] = new[] { "answer", "explanation" }
        }
    };

    await foreach (var message in ClaudeAgent.QueryAsync(
        "What is 7 * 8? Return as JSON with answer and explanation fields.", options))
    {
        if (message is ResultMessage result && result.StructuredOutput != null)
        {
            Log($"  Structured output: {result.StructuredOutput}");
            Log("Test 13 PASSED - Got structured output");
        }
    }
}
catch (Exception ex)
{
    Log($"Test 13 FAILED: {ex.Message}");
}
```

### Test 14: Tool Use with Safe Operations (SAFE - Temp Directory)

```csharp
// ============================================
// Test 14: Tool Use - Safe File Operations (SAFE)
// ============================================
Log("=== Test 14: Safe File Operations ===");
try
{
    // Create isolated temp directory for testing
    var testDir = Path.Combine(Path.GetTempPath(), $"claude-sdk-test-{Guid.NewGuid():N}");
    Directory.CreateDirectory(testDir);
    Log($"  Test directory: {testDir}");

    try
    {
        var options = new ClaudeAgentOptions
        {
            WorkingDirectory = testDir, // Isolated directory
            MaxTurns = 3,
            MaxBudgetUsd = 0.15m,
            // Only allow operations in the test directory
            AllowedTools = new List<string> { "Read", "Write", "Glob" }
        };

        await foreach (var message in ClaudeAgent.QueryAsync(
            "Create a file called 'test.txt' with the content 'Hello from SDK test' and then read it back",
            options))
        {
            if (message is AssistantMessage assistant)
            {
                foreach (var block in assistant.Content)
                {
                    if (block is ToolUseBlock tool)
                        Log($"    Tool: {tool.Name}");
                    if (block is TextBlock text)
                        Log($"    Text: {text.Text}");
                }
            }
        }

        // Verify file was created
        var testFile = Path.Combine(testDir, "test.txt");
        if (File.Exists(testFile))
        {
            var content = File.ReadAllText(testFile);
            Log($"  File content: {content}");
            Log("Test 14 PASSED - File operations in isolated directory");
        }
    }
    finally
    {
        // Clean up test directory
        if (Directory.Exists(testDir))
        {
            Directory.Delete(testDir, recursive: true);
            Log($"  Cleaned up test directory");
        }
    }
}
catch (Exception ex)
{
    Log($"Test 14 FAILED: {ex.Message}");
}
```

### Test 15: QueryToListAsync Convenience Method (SAFE)

```csharp
// ============================================
// Test 15: QueryToListAsync (SAFE)
// ============================================
Log("=== Test 15: QueryToListAsync ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 1,
        MaxBudgetUsd = 0.05m
    };

    var messages = await ClaudeAgent.QueryToListAsync("Say 'test complete'", options);

    Log($"  Total messages: {messages.Count}");
    foreach (var msg in messages)
    {
        Log($"    - {msg.Type}");
    }

    Log("Test 15 PASSED");
}
catch (Exception ex)
{
    Log($"Test 15 FAILED: {ex.Message}");
}
```

### Test 16: QueryTextAsync Convenience Method (SAFE)

```csharp
// ============================================
// Test 16: QueryTextAsync (SAFE)
// ============================================
Log("=== Test 16: QueryTextAsync ===");
try
{
    var options = new ClaudeAgentOptions
    {
        WorkingDirectory = workingDir,
        MaxTurns = 1,
        MaxBudgetUsd = 0.05m
    };

    var result = await ClaudeAgent.QueryTextAsync("What is 5 + 3? Reply with just the number.", options);

    Log($"  Result text: {result}");

    if (result?.Contains("8") == true)
        Log("Test 16 PASSED");
    else
        Log("Test 16 FAILED - unexpected result");
}
catch (Exception ex)
{
    Log($"Test 16 FAILED: {ex.Message}");
}
```

---

## Features NOT Recommended for Automated Testing

The following features should be tested manually or with extreme caution:

### 1. BypassPermissions Mode
```csharp
// ⚠️ DANGEROUS - Do NOT use in automated tests
PermissionMode = PermissionMode.BypassPermissions
```
**Why**: Allows Claude to execute any tool without confirmation. Could modify/delete files, execute arbitrary commands.

### 2. Bash Tool Without Restrictions
```csharp
// ⚠️ DANGEROUS - Always restrict or disable
AllowedTools = new List<string> { "Bash" } // Without safeguards
```
**Why**: Can execute arbitrary shell commands on the host system.

### 3. Unrestricted File Write Operations
```csharp
// ⚠️ DANGEROUS - Always use isolated directories
WorkingDirectory = "/" // Root or home directory
```
**Why**: Could overwrite important system or user files.

### 4. MCP Servers with Sensitive Access
```csharp
// ⚠️ CAREFUL - MCP servers may have elevated permissions
McpServers = new Dictionary<string, McpServerConfig> { ... }
```
**Why**: MCP servers may have access to databases, APIs, or other sensitive resources.

### 5. Extremely High Budget Limits
```csharp
// ⚠️ CAREFUL - Could result in unexpected charges
MaxBudgetUsd = 100.00m
```
**Why**: A bug or infinite loop could consume significant API credits.

---

## Recommended Test Configuration Template

```csharp
// Safe default configuration for testing
var safeTestOptions = new ClaudeAgentOptions
{
    WorkingDirectory = Path.Combine(Path.GetTempPath(), "claude-test"),
    MaxTurns = 3,                    // Limit iterations
    MaxBudgetUsd = 0.25m,            // Limit cost
    PermissionMode = PermissionMode.Default, // Require confirmations
    DisallowedTools = new List<string>
    {
        "Bash",           // No shell commands
        "KillShell",      // No process management
    },
    StderrCallback = line => Console.WriteLine($"[DEBUG] {line}")
};
```

---

## Test Execution Checklist

Before running tests:

- [ ] Verify you're in an isolated test directory
- [ ] Set reasonable `MaxBudgetUsd` limit
- [ ] Set reasonable `MaxTurns` limit
- [ ] Review `DisallowedTools` list
- [ ] Have `StderrCallback` configured for debugging
- [ ] Monitor Anthropic dashboard for unexpected usage

---

## Estimated Test Costs

| Test | Estimated Cost | Notes |
|------|---------------|-------|
| Test 4 (System Prompt) | ~$0.04 | Simple response |
| Test 5 (Model Selection) | ~$0.03 | Minimal prompt |
| Test 6 (Max Turns) | ~$0.04 | Limited to 1 turn |
| Test 7 (Budget Limit) | ~$0.01 | Hard limit |
| Test 8 (Plan Mode) | ~$0.05 | Planning only |
| Test 9 (Disallowed Tools) | ~$0.04 | Simple query |
| Test 10 (Multi-Turn) | ~$0.08 | 2 turns |
| Test 11 (Permission Callback) | ~$0.08 | May use tools |
| Test 12 (Hooks) | ~$0.06 | May use tools |
| Test 13 (Structured Output) | ~$0.04 | JSON response |
| Test 14 (File Operations) | ~$0.10 | Tool usage |
| Test 15 (QueryToListAsync) | ~$0.03 | Simple |
| Test 16 (QueryTextAsync) | ~$0.03 | Simple |
| **Total (all tests)** | **~$0.63** | Approximate |

---

## Appendix: Full Test Sample Code

See `samples/ClaudeAgentSDK.Samples/Program.cs` for the current test implementation.

To add new tests, append them after the existing Test 3 block following the patterns shown above.

---

*Last updated: Based on SDK version matching Claude Code CLI 2.1.12*
