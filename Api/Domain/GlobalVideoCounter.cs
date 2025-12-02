using System;
using Marten.Events.Projections;

namespace Api.Domain;

public class GlobalVideoCounter
{
    public string Id { get; set; } = "GlobalVideoCounter";

    public int TotalVideoViews { get; set; } = 0;
}


public class GlobalVideoCounterProjection : MultiStreamProjection<GlobalVideoCounter, string>
{
    public static readonly string Id = "GlobalVideoCounter";

    public GlobalVideoCounterProjection()
    {
        Identity<VideoViewed>(_ => Id);
    }

    public GlobalVideoCounter Create(VideoViewed _)
    {
        return new GlobalVideoCounter()
        {
            TotalVideoViews = 1
        };
    }

    public void Apply(
        VideoViewed _,
        GlobalVideoCounter view)
    {
        view.TotalVideoViews += 1;
    }
}