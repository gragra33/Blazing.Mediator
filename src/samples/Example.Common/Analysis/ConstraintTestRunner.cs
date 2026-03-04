namespace Example.Common.Analysis;

/// <summary>
/// Constraint test runner for displaying middleware constraint analysis.
/// </summary>
public static class ConstraintTestRunner
{
    /// <summary>
    /// Tests and displays middleware constraint analysis.
    /// </summary>
    /// <param name="catalog">The compile-time mediator type catalog.</param>
    /// <param name="mediatorStatistics">The mediator statistics instance.</param>
    public static void TestConstraintDisplay(IMediatorTypeCatalog catalog, MediatorStatistics mediatorStatistics)
    {
        Console.WriteLine();
        Console.WriteLine("======================================");
        Console.WriteLine("=== MIDDLEWARE CONSTRAINT ANALYSIS ===");
        Console.WriteLine("======================================");
        Console.WriteLine();

        var requestMw  = mediatorStatistics.AnalyzeRequestMiddleware(catalog);
        var notifMw    = mediatorStatistics.AnalyzeNotificationMiddleware(catalog);
        int totalCount = requestMw.Count + notifMw.Count;

        Console.WriteLine($"Found {totalCount} middleware components:");
        Console.WriteLine();

        if (requestMw.Count > 0)
        {
            Console.WriteLine("Request Middleware:");
            foreach (var middleware in requestMw)
                PrintMiddlewareEntry(middleware);
        }

        if (notifMw.Count > 0)
        {
            if (requestMw.Count > 0) Console.WriteLine();
            Console.WriteLine("Notification Middleware:");
            foreach (var middleware in notifMw)
                PrintMiddlewareEntry(middleware);
        }

        Console.WriteLine("======================================");
        Console.WriteLine("=== END CONSTRAINT ANALYSIS ===");
        Console.WriteLine("======================================");
        Console.WriteLine();
    }

    private static void PrintMiddlewareEntry(MiddlewareAnalysis middleware)
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
}