using FluentValidation;
using FluentValidation.Results;
using Framework.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateAttribute(Type modelType) : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        object model = context.ActionArguments.Values.FirstOrDefault(value => value.GetType() == modelType);

        if (model is null)
            throw new FlowException(
                $"You are trying to validate the model with type [{modelType}] but there are no model " +
                $"in arguments. Possibly you have attached this attribute to wrong method or forgot to specify " +
                $"model in that method.");
        
        Type validatorType = typeof(IValidator<>).MakeGenericType(modelType);
        IValidator validator = (IValidator)context.HttpContext.RequestServices.GetService(validatorType)
            ?? throw new ApplicationException("Unable to resolve validator for validation attribute");

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