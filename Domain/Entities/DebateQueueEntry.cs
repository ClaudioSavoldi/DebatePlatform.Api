using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class DebateQueueEntry
    {
        public Guid Id { get; set; }

        public Guid DebateId { get; set; }
        public Debate Debate { get; set; } = null!;

        public Guid UserId { get; set; }

        public MatchSide Side { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
