namespace TaskManagerAPI.API.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidationFilter<T>(IValidator<T> validator) : IAsyncActionFilter where T : class
{
    private readonly IValidator<T> _validator = validator;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.Values.FirstOrDefault(v => v is T) is T model)
        {
            var result = await _validator.ValidateAsync(model);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(result.Errors.Select(e => e.ErrorMessage));
                return;
            }
        }

        await next();
    }
}
