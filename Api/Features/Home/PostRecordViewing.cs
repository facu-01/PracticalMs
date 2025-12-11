using Api.Database;
using Api.Domain;
using Api.Features.Home;
using Api.Middlewares;
using Api.Rendering;
using Marten;
using Wolverine.Http;
using Wolverine.Marten;

namespace Api.Features.Home;

public record RecordViewingsCommand(Guid VideoId, Guid UserId);


public static class PostRecordViewingEndpoint
{
    [WolverinePost("/record-viewings")]
    public static async Task<(IResult, VideoViewed, UserVideoViewed)> Post(
            RecordViewingsCommand _,
            [WriteAggregate] Video video,
            [WriteAggregate] User user,
            IDocumentSession session,
            ComponentRenderer renderer,
            HttpContext context
       )
    {
        var globalVideoCounter = await session.LoadAsync<GlobalVideoCounter>(GlobalVideoCounterProjection.Id);

        var userToLoad = (new Guid[] { InitialDatasets.UsersRegistered[0].Item1, InitialDatasets.UsersRegistered[1].Item1 })
        .Single(i => i != user.Id);

        var otherUser = await session.LoadAsync<User>(userToLoad);

        bool isHtmx = context.IsHtmx();

        string html = await PageHelper.RecordViewings(
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