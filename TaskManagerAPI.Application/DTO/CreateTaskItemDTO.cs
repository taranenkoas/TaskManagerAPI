namespace TaskManagerAPI.Application.DTO;

public class CreateTaskItemDTO
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
