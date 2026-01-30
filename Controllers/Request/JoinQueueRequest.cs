using DebatePlatform.Api.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DebatePlatform.Api.Controllers.Request
{
    public class JoinQueueRequest
    {
        [Required]
        public MatchSide Side { get; set; }
    }
}
