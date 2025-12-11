using System;
using Api.Database;
using Api.Domain;
using Api.Rendering;
using Api.Rendering.Layouts;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;

namespace Api.Features.Home;

public record RecordViewingsCommand(Guid VideoId, Guid UserId);

public static class Routes
{

    private static async Task<string> RecordViewings(
        ComponentRenderer renderer,
        int totalCount,
        int user1VideosWatched,
        int user2VideosWatched,
        bool isPartial = false
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
        }, isPartial: isPartial);
        return bodyHtml;
    }


    [WolverineGet("/")]
    public static async Task<IResult> Get(IDocumentSession session, ComponentRenderer renderer)
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

        string html = await RecordViewings(
            renderer,
            totalCount,
            user1?.VideosWatched ?? 0,
            user2?.VideosWatched ?? 0,
            isPartial: false);


        return Results.Content(html, "text/html");
    }


    [WolverinePost("/record-viewings")]
    public static async Task<(IResult, VideoViewed, UserVideoViewed)> Post(
       RecordViewingsCommand _,
       [FromHeader(Name = "hx-request")] string? hxRequest,
       [WriteAggregate] Video video,
       [WriteAggregate] User user,
       IDocumentSession session,
       ComponentRenderer renderer
   )
    {
        var globalVideoCounter = await session.LoadAsync<GlobalVideoCounter>(GlobalVideoCounterProjection.Id);

        var userToLoad = (new Guid[] { InitialDatasets.UsersRegistered[0].Item1, InitialDatasets.UsersRegistered[1].Item1 })
        .Single(i => i != user.Id);

        var otherUser = await session.LoadAsync<User>(userToLoad);

        bool isHtmx = !string.IsNullOrEmpty(hxRequest);

        string html = await RecordViewings(
            renderer,
            (globalVideoCounter?.TotalVideoViews ?? 0) + 1,
            user1VideosWatched: user.Id == InitialDatasets.UsersRegistered[0].Item1
                ? user.VideosWatched + 1
                : otherUser?.VideosWatched ?? 0,
            user2VideosWatched: user.Id == InitialDatasets.UsersRegistered[1].Item1
                ? user.VideosWatched + 1
                : otherUser?.VideosWatched ?? 0,
            isPartial: isHtmx
        );

        return (
            Results.Content(html, "text/html"),
            new VideoViewed(video.Id, user.Id),
            new UserVideoViewed(user.Id, video.Id)
        );
    }


}
