using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class DebateStatusHistoryItemResponse
    {
        public Guid Id { get; set; }
        public DebateStatus FromStatus { get; set; }
        public DebateStatus ToStatus { get; set; }
        public DateTime ChangedAt { get; set; }

        public Guid? ChangedByUserId { get; set; }
        public string? ChangedByUsernameSnapshot { get; set; }
        public string? ChangedByEmailSnapshot { get; set; }

        public string? Reason { get; set; }
    }
}