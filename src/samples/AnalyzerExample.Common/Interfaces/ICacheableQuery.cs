using Blazing.Mediator;

namespace AnalyzerExample.Common.Interfaces;

public interface ICacheableQuery<out TResponse> : IQuery<TResponse>
{
    string GetCacheKey();
    TimeSpan CacheDuration { get; }
}