namespace TypedMiddlewareExample.Test;

public static class ConstraintTestRunner
{
    public static void TestConstraintDisplay(IServiceProvider serviceProvider)
    {
        Console.WriteLine();
        Console.WriteLine("======================================");
        Console.WriteLine("=== MIDDLEWARE CONSTRAINT ANALYSIS ===");
        Console.WriteLine("======================================");
        Console.WriteLine();

        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var middlewareAnalysis = pipelineInspector.AnalyzeMiddleware(serviceProvider);

        Console.WriteLine($"Found {middlewareAnalysis.Count} middleware components:");
        Console.WriteLine();

        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"* Middleware: {middleware.ClassName}{middleware.TypeParameters}");
            Console.WriteLine($"  Order: {middleware.OrderDisplay}");
            Console.WriteLine($"  Type: {middleware.Type.FullName}");
            if (!string.IsNullOrEmpty(middleware.GenericConstraints))
            {
                Console.WriteLine($"  Constraints: {middleware.GenericConstraints}");
            }
            else
            {
                Console.WriteLine("  Constraints: (none)");
            }
            Console.WriteLine();
        }

        Console.WriteLine("======================================");
        Console.WriteLine("=== END CONSTRAINT ANALYSIS ===");
        Console.WriteLine("======================================");
        Console.WriteLine();
    }
}