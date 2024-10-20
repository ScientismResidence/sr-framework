using FluentValidation;
using FluentValidation.Results;
using Framework;
using Framework.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateAttribute : ActionFilterAttribute
{
    private readonly Type _modelType;

    public ValidateAttribute(Type modelType)
    {
        this._modelType = modelType;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        object model = context.ActionArguments.Values.FirstOrDefault(value => value.GetType() == _modelType);

        if (model is null)
            throw new FlowException(
                $"You are trying to validate the model with type [{_modelType}] but there are no model " +
                $"in arguments. Possibly you have attached this attribute to wrong method or forgot to specify " +
                $"model in that method.");

        IContext serviceProvider = context.HttpContext.RequestServices.GetRequiredService<IContext>();
        Type validatorType = typeof(IValidator<>).MakeGenericType(_modelType);
        IValidator validator = (IValidator)serviceProvider.Get(validatorType);

        ValidationContext<object> validationContext = new ValidationContext<object>(model);
        ValidationResult validationResult = await validator.ValidateAsync(validationContext);

        if (!validationResult.IsValid)
        {
            string message = validationResult.Errors.Select(e => e.ErrorMessage).First();
            context.Result = new BadRequestObjectResult(new ErrorResult
            {
                Message = message
            });
            
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }
}