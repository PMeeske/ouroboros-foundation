namespace Ouroboros.Tools
{
    /// <summary>
    /// Mock MCP server for testing.
    /// </summary>
    public class MockMcpServer
    {
        public List<ToolInfo> ListTools() => new List<ToolInfo>
        {
            new ToolInfo("dsl_suggestion", "Suggest next DSL steps", new { }),
            new ToolInfo("code_analysis", "Analyze C# code", new { })
        };

        public async Task<ToolExecutionResult> ExecuteTool(string toolName, object parameters)
        {
            await Task.Delay(10);
            return new ToolExecutionResult(true, $"Executed {toolName} with result");
        }
    }
}
