namespace TaskManagement.Server.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // The manager leading the team
    public int LeadManagerId { get; set; }
    public User LeadManager { get; set; } = null!;

    // Navigation properties
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
