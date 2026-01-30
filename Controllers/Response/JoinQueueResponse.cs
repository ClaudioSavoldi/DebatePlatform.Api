namespace DebatePlatform.Api.Controllers.Response
{
    public class JoinQueueResponse
    {
        public bool Matched { get; set; }
        public Guid? MatchId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
