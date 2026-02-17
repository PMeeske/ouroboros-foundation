namespace Ouroboros.Core.Interop;

/// <summary>
/// Enhanced demonstration examples
/// </summary>
public static class EnhancedDemo
{
    /// <summary>
    /// Demonstrate Kleisli composition with our enhanced operations
    /// </summary>
    public static async Task RunEnhancedKleisli()
    {
        Console.WriteLine("=== Enhanced Kleisli Composition ===");

        Step<string, string> pipeline = EnhancedSteps.Upper
            .Then(EnhancedSteps.Length)
            .Then(EnhancedSteps.Show);

        string result = await pipeline("hello enhanced kleisli");
        Console.WriteLine($"Result: {result}"); // length=22

        // With error handling
        KleisliResult<string, string, string> safePipeline = EnhancedSteps.SafeParse
            .Then(EnhancedSteps.OnlyPositive.ToResult("Number must be positive"))
            .Map(n => $"Valid positive number: {n}");

        Result<string, string> safeResult1 = await safePipeline("42");
        Result<string, string> safeResult2 = await safePipeline("-5");
        Result<string, string> safeResult3 = await safePipeline("not-a-number");

        Console.WriteLine($"Safe parse '42': {safeResult1}");
        Console.WriteLine($"Safe parse '-5': {safeResult2}");
        Console.WriteLine($"Safe parse 'not-a-number': {safeResult3}");
    }

    /// <summary>
    /// Demonstrate enhanced compatibility pipe with monadic operations
    /// </summary>
    public static async Task RunEnhancedCompatPipe()
    {
        Console.WriteLine("=== Enhanced Compatibility Pipe ===");

        PipeNode<string, string> n1 = EnhancedSteps.Upper.ToCompatNode("Upper");
        PipeNode<string, int> n2 = EnhancedSteps.Length.ToCompatNode("Length");
        PipeNode<int, string> n3 = EnhancedSteps.Show.ToCompatNode("Show");

        // Method-based composition since operator overloading had issues
        PipeNode<string, string> pipeline = n1.Pipe(n2).Pipe(n3);
        string result = await ("enhanced compat pipe" | pipeline);
        Console.WriteLine($"Compat pipe result: {result}");

        // Using fluent pipeline builder
        string fluentResult = await CompatInterop
            .StartPipeline<string>("FluentDemo")
            .AddStep(EnhancedSteps.Upper, "Uppercase")
            .Then(EnhancedSteps.Length, "GetLength")
            .Then(EnhancedSteps.Show, "Format")
            .ExecuteAsync("fluent pipeline demo");

        Console.WriteLine($"Fluent pipeline result: {fluentResult}");

        // With monadic error handling
        PipeNode<string, Result<int, string>> Ouroboros = EnhancedSteps.SafeParse.ToCompatNode("SafeParse");
        Result<int, string> monadicResult = await ("456" | Ouroboros);

        monadicResult.Match(
            success => Console.WriteLine($"Monadic compat success: {success}"),
            error => Console.WriteLine($"Monadic compat error: {error}")
        );
    }

    /// <summary>
    /// Run all enhanced demonstrations
    /// </summary>
    public static async Task RunAllEnhanced()
    {
        await RunEnhancedKleisli();
        await RunEnhancedCompatPipe();

        Console.WriteLine("=== All Enhanced Interop Demonstrations Complete ===");
    }
}