using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System.Diagnostics;

namespace Blazing.Mediator.Benchmarks;

internal sealed class DotTraceDiagnoserAttribute : Attribute, IConfigSource
{
    public DotTraceDiagnoserAttribute()
    {
        var manualConfig = ManualConfig.CreateEmpty();
        manualConfig.AddDiagnoser(new DotTraceDiagnoser());
        Config = manualConfig;
    }

    public IConfig Config { get; }
}

internal sealed class DotTraceDiagnoser : IDiagnoser
{
    private const string DotTraceExecutableNotFoundErrorMessage = "dotTrace executable was not found. " +
                                                                  "Make sure it is part of the PATH or install JetBrains.dotTrace.GlobalTools";

    private readonly string _saveLocation = $"C:\\temp\\BlazingMediator\\{DateTimeOffset.Now.UtcDateTime:yyyy-MM-dd-HH_mm_ss}.bench.dtp";

    /// <inheritdoc />
    public RunMode GetRunMode(BenchmarkCase benchmarkCase) => RunMode.ExtraRun;

    /// <inheritdoc />
    public void Handle(HostSignal signal, DiagnoserActionParameters parameters)
    {
        if (signal != HostSignal.BeforeActualRun)
        {
            return;
        }

        try
        {
            if (!CanRunDotTrace())
            {
                Console.WriteLine(DotTraceExecutableNotFoundErrorMessage);
                return;
            }

            // The directory must exist or an error is thrown by dotTrace.
            Directory.CreateDirectory(_saveLocation);
            RunDotTrace(parameters);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
            throw;
        }
    }

    private void RunDotTrace(DiagnoserActionParameters parameters)
    {
        var dotTrace = new Process
        {
            StartInfo = PrepareProcessStartInfo(parameters)
        };
        dotTrace.ErrorDataReceived += (_, eventArgs) => Console.Error.WriteLine(eventArgs.Data);
        dotTrace.OutputDataReceived += (_, eventArgs) => Console.WriteLine(eventArgs.Data);
        dotTrace.Start();
        dotTrace.BeginErrorReadLine();
        dotTrace.BeginOutputReadLine();
        dotTrace.Exited += (_, _) => dotTrace.Dispose();
    }

    private ProcessStartInfo PrepareProcessStartInfo(DiagnoserActionParameters parameters)
    {
        return new ProcessStartInfo(
            "dottrace",
            $"attach {parameters.Process.Id} --save-to={_saveLocation}")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
    }

    /// <inheritdoc />
    public IEnumerable<Metric> ProcessResults(DiagnoserResults results) => [];

    /// <inheritdoc />
    public void DisplayResults(ILogger logger) { }

    /// <inheritdoc />
    public IEnumerable<ValidationError> Validate(ValidationParameters validationParameters) =>
        [];

    /// <inheritdoc />
    public IEnumerable<string> Ids => [nameof(DotTraceDiagnoser)];

    /// <inheritdoc />
    public IEnumerable<IExporter> Exporters => [];

    public IEnumerable<IAnalyser> Analysers { get; } = [];

    private static bool CanRunDotTrace()
    {
        try
        {
            var startInfo = new ProcessStartInfo("dottrace")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}