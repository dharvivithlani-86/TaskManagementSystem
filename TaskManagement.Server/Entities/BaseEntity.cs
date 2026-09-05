namespace TaskManagement.Server.Entities;

/// <summary>
/// Common abstract base entity containing standard primary key and audit trail properties.
/// All domain tables inherit from this class to guarantee consistent schema and auditing.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "System";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
