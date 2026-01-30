using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class PublicMatchListItemResponse
    {
        public Guid MatchId { get; set; }
        public Guid DebateId { get; set; }
        public string DebateTitle { get; set; } = string.Empty;

        public MatchPhase Phase { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ProCount { get; set; }
        public int ControCount { get; set; }
        public int TotalVotes { get; set; }
    }
}
