using Api.Database;
using Api.Domain;
using Api.Features.Home;
using Api.Rendering;
using Marten;
using Wolverine.Http;

namespace Api.Features.Home;

public static class GetEndpoint
{
    [WolverineGet("/")]
    public static async Task<IResult> Get(IDocumentSession session,
    ComponentRenderer renderer)
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

        string html = await PageHelper.RecordViewings(
            renderer,
            totalCount,
            user1?.VideosWatched ?? 0,
            user2?.VideosWatched ?? 0,
            isPartial: false);


        return Results.Content(html, "text/html");
    }
}