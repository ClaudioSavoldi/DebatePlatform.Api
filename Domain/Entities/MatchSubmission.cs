using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class MatchSubmission
    {
        public Guid Id { get; set; }

        public Guid MatchId { get; set; }
        public DebateMatch Match { get; set; } = null!;

        public Guid UserId { get; set; }
        public SubmissionPhase Phase { get; set; }

        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSubmitted { get; set; } = false;
        public DateTime? SubmittedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
