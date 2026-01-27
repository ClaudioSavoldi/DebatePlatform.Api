using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Domain.Entities
{
    public class Debate
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public DebateStatus Status { get; set; } = DebateStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<DebateStatusHistory> StatusHistory { get; set; } = new List<DebateStatusHistory>();
    }
}
