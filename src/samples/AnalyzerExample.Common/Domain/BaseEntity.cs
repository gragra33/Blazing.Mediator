namespace AnalyzerExample.Common.Domain;

/// <summary>
/// Base class for all entities in the domain
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}