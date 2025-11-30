namespace Api.Database;

public class Video
{
    public long Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TranscodingStatus { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}
