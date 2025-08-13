namespace TaskManagerAPI.Domain.Entities;

public enum TaskStatus { New = 0, InProgress = 1, Done = 2 }

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? OwnerId { get; set; }
    public User? Owner { get; set; }
}
