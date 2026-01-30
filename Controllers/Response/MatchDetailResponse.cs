using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class MatchDetailResponse
    {
        public Guid Id { get; set; }
        public Guid DebateId { get; set; }
        public MatchPhase Phase { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid ProUserId { get; set; }
        public Guid ControUserId { get; set; }

        public int ProCount { get; set; }
        public int ControCount { get; set; }
        public int TotalVotes { get; set; }

        public DateTime? VotingEndsAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public Guid? WinnerUserId { get; set; }
        public string? WinnerUsername { get; set; }
        public bool IsDraw { get; set; }



        public List<MatchSubmissionResponse> OpeningSubmissions { get; set; } = new();
        public List<MatchSubmissionResponse> RebuttalSubmissions { get; set; } = new();
    }
}
