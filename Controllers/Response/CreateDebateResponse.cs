using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Response
{
    public class CreateDebateResponse
    {
        public Guid Id { get; set; }
        public DebateStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
