using System.Text.Json;
using System.Text.Json.Serialization;
using NodaTime.Serialization.SystemTextJson;

namespace LeoMiniApi.Core;

public static class Setup
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
    }

    public static void ConfigureServices(this IServiceCollection services, bool isDevelopment)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = isDevelopment;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        });
    }
}
