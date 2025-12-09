using System;
using Api.Rendering;
using Wolverine.Http;

namespace Api.Features.Home;

public static class Route
{

    [WolverineGet("/")]
    public static async Task<IResult> Get(ComponentRenderer renderer)
    {

        var bodyHtml = await renderer.RenderAsync<Page>(builder =>
        {

            builder.Add(c => c.Message, "Welcome to the New Home Page");
            builder.Add(c => c.UserId, "1234");
        });

        return Results.Content(bodyHtml, "text/html");
    }


}
