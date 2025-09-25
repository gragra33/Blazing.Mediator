using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Performance benchmarks for stream processing variations in Blazing.Mediator.
/// Measures the performance of different streaming scenarios and configurations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class StreamProcessingBenchmarks
{
    private IMediator _mediatorNoMiddleware = null!;
    private IMediator _mediatorWithMiddleware = null!;

    private SmallStreamRequest _smallStreamRequest = null!;
    private MediumStreamRequest _mediumStreamRequest = null!;
    private LargeStreamRequest _largeStreamRequest = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallStreamRequest = new SmallStreamRequest { Count = 10 };
        _mediumStreamRequest = new MediumStreamRequest { Count = 1000 };
        _largeStreamRequest = new LargeStreamRequest { Count = 10000 };

        // Setup mediator WITHOUT stream middleware
        var servicesNoMiddleware = new ServiceCollection();
        servicesNoMiddleware.AddMediator(typeof(StreamProcessingBenchmarks).Assembly);
        var providerNoMiddleware = servicesNoMiddleware.BuildServiceProvider();
        _mediatorNoMiddleware = providerNoMiddleware.GetRequiredService<IMediator>();

        // Setup mediator WITH stream middleware
        var servicesWithMiddleware = new ServiceCollection();
        servicesWithMiddleware.AddMediator(typeof(StreamProcessingBenchmarks).Assembly);
        servicesWithMiddleware.AddScoped(typeof(IRequestMiddleware<,>), typeof(StreamLoggingMiddleware<,>));
        var providerWithMiddleware = servicesWithMiddleware.BuildServiceProvider();
        _mediatorWithMiddleware = providerWithMiddleware.GetRequiredService<IMediator>();
    }

    #region Small Stream Benchmarks (10-100 items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Small_Streams")]
    public async Task<int> SmallStream_NoMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(_smallStreamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Small_Streams")]
    public async Task<int> SmallStream_WithMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorWithMiddleware.SendStream(_smallStreamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Medium Stream Benchmarks (1K items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Medium_Streams")]
    public async Task<int> MediumStream_NoMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(_mediumStreamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Medium_Streams")]
    public async Task<int> MediumStream_WithMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorWithMiddleware.SendStream(_mediumStreamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Large Stream Benchmarks (10K items)

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large_Streams")]
    public async Task<int> LargeStream_NoMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(_largeStreamRequest))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Large_Streams")]
    public async Task<int> LargeStream_WithMiddleware()
    {
        var count = 0;
        await foreach (var item in _mediatorWithMiddleware.SendStream(_largeStreamRequest))
        {
            count++;
        }
        return count;
    }

    #endregion

    #region Memory Efficient Stream Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Memory_Efficient_Streams")]
    public async Task<int> MemoryEfficientStream_Small()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(new MemoryEfficientStreamRequest { Count = 100 }))
        {
            count++;
            // Process immediately without storing
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Memory_Efficient_Streams")]
    public async Task<int> MemoryEfficientStream_Large()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(new MemoryEfficientStreamRequest { Count = 5000 }))
        {
            count++;
            // Process immediately without storing
        }
        return count;
    }

    #endregion

    #region Fast Processing Stream Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Fast_Processing_Streams")]
    public async Task<int> FastProcessingStream_NoDelay()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(new FastStreamRequest { Count = 1000 }))
        {
            count++;
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Fast_Processing_Streams")]
    public async Task<int> FastProcessingStream_WithBatching()
    {
        var items = new List<string>();
        await foreach (var item in _mediatorNoMiddleware.SendStream(new FastStreamRequest { Count = 1000 }))
        {
            items.Add(item);
            if (items.Count >= 100)
            {
                // Process batch
                items.Clear();
            }
        }
        return items.Count;
    }

    #endregion

    #region Stream Processing Variations

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Stream_Variations")]
    public async Task<int> StringStream_Processing()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(new StringStreamRequest { Count = 500 }))
        {
            count += item.Length; // Some processing
        }
        return count;
    }

    [Benchmark]
    [BenchmarkCategory("Stream_Variations")]
    public async Task<int> ObjectStream_Processing()
    {
        var count = 0;
        await foreach (var item in _mediatorNoMiddleware.SendStream(new ObjectStreamRequest { Count = 500 }))
        {
            count += item.Id; // Some processing
        }
        return count;
    }

    #endregion

    #region Test Classes and Handlers

    public class SmallStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class SmallStreamHandler : IStreamRequestHandler<SmallStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(SmallStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken); // Small delay
                yield return $"Small-{i}";
            }
        }
    }

    public class MediumStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class MediumStreamHandler : IStreamRequestHandler<MediumStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(MediumStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (i % 100 == 0) // Occasional delay
                    await Task.Delay(1, cancellationToken);
                yield return $"Medium-{i}";
            }
        }
    }

    public class LargeStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class LargeStreamHandler : IStreamRequestHandler<LargeStreamRequest, string>
    {
        public async IAsyncEnumerable<string> Handle(LargeStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (i % 1000 == 0) // Rare delay
                    await Task.Delay(1, cancellationToken);
                yield return $"Large-{i}";
            }
        }
    }

    public class MemoryEfficientStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class MemoryEfficientStreamHandler : IStreamRequestHandler<MemoryEfficientStreamRequest, string>
    {
        public IAsyncEnumerable<string> Handle(MemoryEfficientStreamRequest request,
            CancellationToken cancellationToken = default)
        {
            return HandleInternal(request, cancellationToken);
        }

        private static async IAsyncEnumerable<string> HandleInternal(MemoryEfficientStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"Efficient-{i}";
                await Task.Yield(); // Yield control
            }
        }
    }

    public class FastStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class FastStreamHandler : IStreamRequestHandler<FastStreamRequest, string>
    {
        public IAsyncEnumerable<string> Handle(FastStreamRequest request,
            CancellationToken cancellationToken = default)
        {
            return HandleInternal(request, cancellationToken);
        }

        private static async IAsyncEnumerable<string> HandleInternal(FastStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"Fast-{i}";
                await Task.Yield(); // Yield control
            }
        }
    }

    public class StringStreamRequest : IStreamRequest<string>
    {
        public int Count { get; set; }
    }

    public class StringStreamHandler : IStreamRequestHandler<StringStreamRequest, string>
    {
        public IAsyncEnumerable<string> Handle(StringStreamRequest request,
            CancellationToken cancellationToken = default)
        {
            return HandleInternal(request, cancellationToken);
        }

        private static async IAsyncEnumerable<string> HandleInternal(StringStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return $"StringData-{i}-{Guid.NewGuid()}";
                await Task.Yield(); // Yield control
            }
        }
    }

    public class StreamDataObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ObjectStreamRequest : IStreamRequest<StreamDataObject>
    {
        public int Count { get; set; }
    }

    public class ObjectStreamHandler : IStreamRequestHandler<ObjectStreamRequest, StreamDataObject>
    {
        public IAsyncEnumerable<StreamDataObject> Handle(ObjectStreamRequest request,
            CancellationToken cancellationToken = default)
        {
            return HandleInternal(request, cancellationToken);
        }

        private static async IAsyncEnumerable<StreamDataObject> HandleInternal(ObjectStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new StreamDataObject
                {
                    Id = i,
                    Name = $"Object-{i}",
                    Timestamp = DateTime.UtcNow
                };
                await Task.Yield(); // Yield control
            }
        }
    }

    #endregion

    #region Stream Middleware

    public class StreamLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public int Order => 0;

        public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // For stream requests, we just pass through to the handler
            // Stream-specific middleware would need to be implemented differently
            return await next();
        }
    }

    #endregion
}