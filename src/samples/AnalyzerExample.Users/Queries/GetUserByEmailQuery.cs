using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

public class GetUserByEmailQuery : IUserQuery<UserDetailDto?>, ICacheableQuery<UserDetailDto?>
{
    public string Email { get; set; } = string.Empty;
    public bool IncludeInactive { get; set; } = false;
    
    public string GetCacheKey() => $"user_email_{Email.ToLowerInvariant()}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}