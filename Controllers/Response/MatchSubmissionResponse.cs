using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class MatchSubmissionResponse
    {
        public Guid UserId { get; set; }
        public SubmissionPhase Phase { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
