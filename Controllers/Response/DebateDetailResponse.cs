using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class DebateDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public DebateStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

        // Voti aggregati
        public int ProCount { get; set; }
        public int ControCount { get; set; }
        public int TotalVotes { get; set; }

        // Verbale moderazione
        public List<DebateStatusHistoryItemResponse> StatusHistory { get; set; } = new();
    }
}
