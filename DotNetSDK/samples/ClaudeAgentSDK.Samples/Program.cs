using ClaudeAgentSDK;
using ClaudeAgentSDK.Models;

// Example 1: Simple one-shot query
Console.WriteLine("=== Example 1: Simple Query ===");
await foreach (var message in ClaudeAgent.QueryAsync("What is 2 + 2?"))
{
    switch (message)
    {
        case AssistantMessage assistant:
            foreach (var block in assistant.Content)
            {
                if (block is TextBlock text)
                {
                    Console.Write(text.Text);
                }
            }
            break;

        case ResultMessage result:
            Console.WriteLine();
            Console.WriteLine($"[Session: {result.SessionId}, Turns: {result.NumTurns}]");
            break;
    }
}

// Example 2: Query with options
Console.WriteLine("\n=== Example 2: Query with Options ===");
var options = new ClaudeAgentOptions
{
    Model = "claude-sonnet-4-20250514",
    MaxTurns = 3,
    PermissionMode = PermissionMode.AcceptEdits
};

var resultText = await ClaudeAgent.QueryTextAsync(
    "Explain async/await in C# in one sentence",
    options);
Console.WriteLine(resultText);

// Example 3: Interactive client
Console.WriteLine("\n=== Example 3: Interactive Client ===");
await using var client = ClaudeAgent.CreateClient();
await client.ConnectAsync("Hello! Remember my name is Alice.");

await foreach (var message in client.ReceiveResponseAsync())
{
    if (message is AssistantMessage assistant)
    {
        foreach (var block in assistant.Content)
        {
            if (block is TextBlock text)
            {
                Console.Write(text.Text);
            }
        }
    }
}
Console.WriteLine();

// Follow-up in same session
await client.QueryAsync("What's my name?");
await foreach (var message in client.ReceiveResponseAsync())
{
    if (message is AssistantMessage assistant)
    {
        foreach (var block in assistant.Content)
        {
            if (block is TextBlock text)
            {
                Console.Write(text.Text);
            }
        }
    }
}
Console.WriteLine();

// Example 4: Handling tool use
Console.WriteLine("\n=== Example 4: Tool Use ===");
await foreach (var message in ClaudeAgent.QueryAsync("What files are in the current directory?"))
{
    switch (message)
    {
        case AssistantMessage assistant:
            foreach (var block in assistant.Content)
            {
                switch (block)
                {
                    case TextBlock text:
                        Console.WriteLine($"[Text] {text.Text}");
                        break;
                    case ToolUseBlock tool:
                        Console.WriteLine($"[Tool] {tool.Name}: {tool.Id}");
                        break;
                }
            }
            break;

        case UserMessage user when user.GetContentBlocks() != null:
            foreach (var block in user.GetContentBlocks()!)
            {
                if (block is ToolResultBlock result)
                {
                    Console.WriteLine($"[Result] Tool {result.ToolUseId}: {(result.IsError == true ? "Error" : "Success")}");
                }
            }
            break;
    }
}

// Example 5: Custom permission callback
Console.WriteLine("\n=== Example 5: Custom Permission Callback ===");
var optionsWithCallback = new ClaudeAgentOptions
{
    CanUseTool = async (request) =>
    {
        Console.WriteLine($"Permission requested for tool: {request.ToolName}");

        // Deny any file write operations
        if (request.ToolName.Contains("Write", StringComparison.OrdinalIgnoreCase))
        {
            return new PermissionResultDeny
            {
                Message = "Write operations are not allowed in this example"
            };
        }

        return new PermissionResultAllow();
    }
};

await foreach (var message in ClaudeAgent.QueryAsync(
    "List files in the current directory",
    optionsWithCallback))
{
    if (message is AssistantMessage assistant)
    {
        foreach (var block in assistant.Content)
        {
            if (block is TextBlock text)
            {
                Console.Write(text.Text);
            }
        }
    }
}

Console.WriteLine("\n\nDone!");
