
namespace Api.Middlewares;

public class CorrelationMiddleware
{
    public static void Before(HttpContext httpContext)
    {
        httpContext.Response.Headers.Append("X-Correlation-ID", Guid.NewGuid().ToString());
    }
}
