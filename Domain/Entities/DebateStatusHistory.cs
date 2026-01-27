using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class DebateStatusHistory
    {
        public Guid Id { get; set; }

        public Guid DebateId { get; set; }
        public Debate Debate { get; set; } = null!;

        public DebateStatus FromStatus { get; set; }
        public DebateStatus ToStatus { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public Guid? ChangedByUserId { get; set; }


        // Snapshot,rimangono anche se lo User viene eliminato
        public string? ChangedByUsernameSnapshot { get; set; }
        public string? ChangedByEmailSnapshot { get; set; }

        public string? Reason { get; set; }
    }
}
