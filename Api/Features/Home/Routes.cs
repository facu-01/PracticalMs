using System;
using Api.Database;
using Api.Domain;
using Api.Rendering;
using Api.Rendering.Layouts;
using Marten;
using Wolverine.Http;
using Wolverine.Marten;

namespace Api.Features.Home;

public record RecordViewingsCommand(Guid VideoId, Guid UserId);

public static class Routes
{

    private static async Task<string> RecordViewingsFragment(
        ComponentRenderer renderer,
        int totalCount,
        int user1VideosWatched,
        int user2VideosWatched
    )
    {

        var mockVideoId = InitialDatasets.VideosUploaded[0].Item1.ToString();

        var bodyHtml = await renderer.RenderAsync<Page>(builder =>
        {
            builder.Add(c => c.VideosWatched, totalCount);
            builder.Add(c => c.User1Id, InitialDatasets.UsersRegistered[0].Item1.ToString());
            builder.Add(c => c.User1VideosWatched, user1VideosWatched);
            builder.Add(c => c.User2Id, InitialDatasets.UsersRegistered[1].Item1.ToString());
            builder.Add(c => c.User2VideosWatched, user2VideosWatched);
            builder.Add(c => c.VideoId, mockVideoId);
        });
        return bodyHtml;
    }


    [WolverineGet("/")]
    public static async Task<IResult> Get(ComponentRenderer renderer, IDocumentSession session)
    {

        var batchQuery = session.CreateBatchQuery();
        var globalVideoCounterQuery = batchQuery.Load<GlobalVideoCounter>(GlobalVideoCounterProjection.Id);
        var user1Query = batchQuery.Load<User>(InitialDatasets.UsersRegistered[0].Item1);
        var user2Query = batchQuery.Load<User>(InitialDatasets.UsersRegistered[1].Item1);

        await batchQuery.Execute();

        var user1 = await user1Query;
        var user2 = await user2Query;
        var globalVideoCounter = await globalVideoCounterQuery;
        var totalCount = globalVideoCounter?.TotalVideoViews ?? 0;

        string fragment = await RecordViewingsFragment(
            renderer,
            totalCount,
            user1?.VideosWatched ?? 0,
            user2?.VideosWatched ?? 0);


        var fullPageHtml = await renderer.RenderLayoutAsync<Layout>(
            bodyContent: fragment
        );


        return Results.Content(fullPageHtml, "text/html");
    }


    [WolverinePost("/record-viewings")]
    public static async Task<(IResult, VideoViewed, UserVideoViewed)> Post(
       RecordViewingsCommand _,
       [WriteAggregate] Video video,
       [WriteAggregate] User user,
       ComponentRenderer renderer,
       IDocumentSession session
   )
    {
        var globalVideoCounter = await session.LoadAsync<GlobalVideoCounter>(GlobalVideoCounterProjection.Id);

        var userToLoad = (new Guid[] { InitialDatasets.UsersRegistered[0].Item1, InitialDatasets.UsersRegistered[1].Item1 })
        .Single(i => i != user.Id);

        var otherUser = await session.LoadAsync<User>(userToLoad);


        string bodyHtml = await RecordViewingsFragment(
            renderer,
            (globalVideoCounter?.TotalVideoViews ?? 0) + 1,
            user1VideosWatched: user.Id == InitialDatasets.UsersRegistered[0].Item1
                ? user.VideosWatched + 1
                : otherUser?.VideosWatched ?? 0,
            user2VideosWatched: user.Id == InitialDatasets.UsersRegistered[1].Item1
                ? user.VideosWatched + 1
                : otherUser?.VideosWatched ?? 0
        );

        return (
            Results.Content(bodyHtml, "text/html"),
            new VideoViewed(video.Id, user.Id),
            new UserVideoViewed(user.Id, video.Id)
        );
    }


}
