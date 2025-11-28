using Wolverine.Http;
using Api.Rendering;
using Api.Rendering.Layouts;


namespace Api.Features.HomePage;

public static class HomePageEndpoint
{
    [WolverineGet("/")]
    public static async Task<IResult> Get(ComponentRenderer renderer)
    {


        var bodyHtml = await renderer.RenderAsync<HomePageComponent>(builder =>
        {
            builder.Add(c => c.User, "Guest");
        });

        var fullPageHtml = await renderer.RenderLayoutAsync<MainLayout>(
            bodyContent: bodyHtml,
            configure: builder =>
            {
                builder.Add(c => c.Message, "Welcome to the Home Page")
                .Add(c => c.UserId, "1234")
                ;
            }
        );


        return Results.Content(fullPageHtml, "text/html; charset=utf-8");
    }
}
