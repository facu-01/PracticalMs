using System;
using Api.Database;
using Api.Rendering;

namespace Api.Features.Home;

public static class PageHelper
{
    public static async Task<string> RecordViewings(
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
}
