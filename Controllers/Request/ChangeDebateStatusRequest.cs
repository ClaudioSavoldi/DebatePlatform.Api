using System.ComponentModel.DataAnnotations;
using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Request
{
    public class ChangeDebateStatusRequest
    {
        [Required]
        public DebateStatus NewStatus { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
