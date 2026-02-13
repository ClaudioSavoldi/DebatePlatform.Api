using DebatePlatform.Api.Domain.Enums;

public class MyQueueItemResponse
{
    public Guid DebateId { get; set; }
    public string DebateTitle { get; set; } = string.Empty;
    public MatchSide Side { get; set; }         // Pro/Contro
    public DateTime JoinedAt { get; set; }
}
