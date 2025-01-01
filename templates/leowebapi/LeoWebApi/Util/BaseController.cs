using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace LeoWebApi.Util;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request)
        where TValidator : AbstractValidator<TRequest>, new()
        where TRequest : notnull
    {
        var validator = new TValidator();

        return ValidateRequest(request, validator);
    }

    protected static bool ValidateRequest<TValidator, TRequest>(TRequest request, TValidator validator)
        where TValidator : AbstractValidator<TRequest>
        where TRequest : notnull =>
        validator.Validate(request).IsValid;
}
