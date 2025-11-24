using Wolverine.Http;

namespace Api.Middlewares;

public class CorrelationMiddleware
{
    public static IResult After(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("X-Correlation-ID", Guid.NewGuid().ToString());

        return WolverineContinue.Result();
    }

}
