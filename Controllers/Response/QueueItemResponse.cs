using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class QueueItemResponse
    {
        public Guid DebateId { get; set; }
        public string DebateTitle { get; set; } = string.Empty;
        public MatchSide Side { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
