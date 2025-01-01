using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime.Text;

namespace LeoWebApi.Util;

public sealed class LocalDateModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        string? dateString = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(dateString))
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Empty date");

            return Task.CompletedTask;
        }

        ParseResult<LocalDate> parseResult = LocalDatePattern.Iso.Parse(dateString);
        if (!parseResult.Success)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format");

            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(parseResult.Value);

        return Task.CompletedTask;
    }
}

public sealed class LocalDateModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(LocalDate)
            ? new LocalDateModelBinder()
            : null;
}
