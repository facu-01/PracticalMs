using Api.Features.RegisterUsers;
using Api.Middlewares;
using Api.Rendering;
using Wolverine.Http;

namespace Api.Features.RegisterUsers;

public static class GetFormEndpoint
{
    [WolverineGet("/register")]
    public static async Task<IResult> GetRegister(
        HttpContext context,
        ComponentRenderer renderer
    )
    {

        var html = await renderer.RenderAsync<RegisterForm>(isPartial: context.IsHtmx());

        return Results.Content(html, "text/html");
    }
}