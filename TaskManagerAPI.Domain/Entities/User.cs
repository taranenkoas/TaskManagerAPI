namespace TaskManagerAPI.Domain.Entities;

using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public string? FullName { get; set; }

    public ICollection<TaskItem> TaskItems { get; set; } = [];
}
