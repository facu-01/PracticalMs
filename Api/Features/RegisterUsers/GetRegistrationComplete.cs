using Api.Middlewares;
using Api.Rendering;
using Wolverine.Http;
namespace Api.Features.RegisterUsers;

public static class GetRegistrationCompleteEndpoint
{
    [WolverineGet("/register/registration-complete")]
    public static async Task<IResult> GetRegistrationComplete(
        ComponentRenderer renderer,
        HttpContext context
    )
    {
        var html = await renderer.RenderAsync<RegisterComplete>(isPartial: context.IsHtmx());

        return Results.Content(html, "text/html");
    }
}