namespace Example.Common.Analysis;

/// <summary>
/// Constraint test runner for displaying middleware constraint analysis.
/// </summary>
public static class ConstraintTestRunner
{
    /// <summary>
    /// Tests and displays middleware constraint analysis.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve services from.</param>
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
            // Clean class name to remove backticks
            var cleanClassName = middleware.ClassName;
            if (cleanClassName.Contains('`'))
            {
                var backtickIndex = cleanClassName.IndexOf('`');
                cleanClassName = cleanClassName[..backtickIndex];
            }

            // Clean type parameters to remove backticks
            var cleanTypeParameters = middleware.TypeParameters;
            if (cleanTypeParameters.Contains('`'))
            {
                cleanTypeParameters = System.Text.RegularExpressions.Regex.Replace(
                    cleanTypeParameters, @"`\d+", "");
            }

            // Clean the full type name to remove backticks
            var cleanTypeName = middleware.Type.FullName ?? middleware.Type.Name;
            if (cleanTypeName.Contains('`'))
            {
                cleanTypeName = System.Text.RegularExpressions.Regex.Replace(
                    cleanTypeName, @"`\d+", "");
            }

            Console.WriteLine($"* Middleware: {cleanClassName}{cleanTypeParameters}");
            Console.WriteLine($"  Order: {middleware.OrderDisplay}");
            Console.WriteLine($"  Type: {cleanTypeName}");
            if (!string.IsNullOrEmpty(middleware.GenericConstraints))
            {
                // Also clean constraints
                var cleanConstraints = middleware.GenericConstraints;
                if (cleanConstraints.Contains('`'))
                {
                    cleanConstraints = System.Text.RegularExpressions.Regex.Replace(
                        cleanConstraints, @"`\d+", "");
                }
                Console.WriteLine($"  Constraints: {cleanConstraints}");
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