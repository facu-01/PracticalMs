using Wolverine.Http;
using Api.Rendering;
using Api.Rendering.Layouts;
using Wolverine.Marten;
using Api.Domain;
using Marten;
using Api.Database;


namespace Api.Features.HomePage;

public static class HomePageEndpoint
{
    [WolverineGet("/")]
    public static async Task<IResult> Get(ComponentRenderer renderer, IDocumentSession session)
    {
        var globalVideoCounter = await session.LoadAsync<GlobalVideoCounter>(GlobalVideoCounterProjection.Id);


        var batchQuery = session.CreateBatchQuery();

        var user1Query = batchQuery.Load<User>(InitialDatasets.UsersRegistered[0].Item1);
        var user2Query = batchQuery.Load<User>(InitialDatasets.UsersRegistered[1].Item1);

        await batchQuery.Execute();

        var user1 = await user1Query;
        var user2 = await user2Query;

        var totalCount = globalVideoCounter?.TotalVideoViews ?? 0;
        var mockVideoId = InitialDatasets.VideosUploaded[0].Item1.ToString();

        var bodyHtml = await renderer.RenderAsync<Page>(builder =>
        {
            builder.Add(c => c.VideosWatched, totalCount);
            builder.Add(c => c.User1Id, InitialDatasets.UsersRegistered[0].Item1.ToString());
            builder.Add(c => c.User1VideosWatched, user1?.VideosWatched ?? 0);
            builder.Add(c => c.User2Id, InitialDatasets.UsersRegistered[1].Item1.ToString());
            builder.Add(c => c.User2VideosWatched, user2?.VideosWatched ?? 0);
            builder.Add(c => c.VideoId, mockVideoId);
        });

        var fullPageHtml = await renderer.RenderLayoutAsync<MainLayout>(
            bodyContent: bodyHtml,
            configure: builder =>
            {
                builder.Add(c => c.Message, "Welcome to the Home Page")
                .Add(c => c.UserId, "1234");
            }
        );

        return Results.Content(fullPageHtml, "text/html");
    }
}
