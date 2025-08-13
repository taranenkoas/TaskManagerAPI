namespace TaskManagerAPI.Application.Mappings;

using AutoMapper;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Domain.Entities;

public class TaskItemProfile : Profile
{
    public TaskItemProfile()
    {
        CreateMap<TaskItem, TaskItemDTO>().ReverseMap();
        CreateMap<CreateTaskItemDTO, TaskItem>();
    }
}
