using NodaTime.Text;

namespace LeoWebApi.Util;

public sealed class LocalDateRouteConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values,
                      RouteDirection routeDirection)
    {
        if (!values.TryGetValue(routeKey, out var routeValue)
            || routeValue is null)
        {
            return false;
        }

        var parseResult = LocalDatePattern.Iso.Parse(routeValue.ToString() ?? string.Empty);

        return parseResult.Success;
    }
}
