using System;
using Api.Database;
using Api.Domain;
using Api.Rendering;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;

namespace Api.Features.RecordViewings;

public record RecordViewingsCommand(Guid VideoId, Guid UserId);

public static class Route
{

    [WolverineGet("/record-viewings")]
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

        return Results.Content(bodyHtml, "text/html");
    }


    [WolverinePost("/record-viewings")]
    public static (IResult, VideoViewed, UserVideoViewed) Post(
        RecordViewingsCommand _,
        [WriteAggregate] Video video,
        [WriteAggregate] User user
    )
    {
        return (
            Results.Redirect("/record-viewings"),
            new VideoViewed(video.Id, user.Id),
            new UserVideoViewed(user.Id, video.Id)
        );
    }

}
