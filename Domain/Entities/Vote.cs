using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class Vote
    {
        public Guid Id { get; set; }

        public Guid DebateId { get; set; }
        public Debate Debate { get; set; } = null!;

        public Guid UserId { get; set; }

        public VoteValue Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
