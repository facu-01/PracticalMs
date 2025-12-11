namespace Api.Middlewares;

public class HtmxMiddleware
{
    public static void Before(HttpContext context)
    {
        context.Items["IsHtmx"] = context.Request.Headers.ContainsKey("HX-Request");
    }
}

public static class HtmxHttpContextExtensions
{
    public static bool IsHtmx(this HttpContext context)
    {
        return context.Items.TryGetValue("IsHtmx", out var value) && value is true;
    }
}
