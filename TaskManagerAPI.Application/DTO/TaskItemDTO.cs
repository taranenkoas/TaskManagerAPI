namespace TaskManagerAPI.Application.DTO;

public class TaskItemDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
