namespace TaskManagerAPI.Application.Validators;

using FluentValidation;
using TaskManagerAPI.Application.DTO;

public class CreateTaskItemDTOValidator : AbstractValidator<CreateTaskItemDTO>
{
    public CreateTaskItemDTOValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название задачи обязательно.")
            .MaximumLength(5).WithMessage("Название не может быть длиннее 100 символов.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не может быть длиннее 500 символов.");

        RuleFor(x => x.Status)
            .Must(status => Enum.IsDefined(typeof(TaskStatus), status))
            .WithMessage("Недопустимый статус задачи.");
    }
}
