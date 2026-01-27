using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class DebateListItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DebateStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
