using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class DebateMatch
    {
        public Guid Id { get; set; }

        public Guid DebateId { get; set; }
        public Debate Debate { get; set; } = null!;

        public MatchPhase Phase { get; set; } = MatchPhase.Opening;

        public Guid ProUserId { get; set; }
        public Guid ControUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VotingEndsAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public ICollection<MatchSubmission> Submissions { get; set; } = new List<MatchSubmission>();
    }
}
