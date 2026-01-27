using System.ComponentModel.DataAnnotations;
using DebatePlatform.Api.Domain.Enums;

namespace DebatePlatform.Api.Controllers.Request
{
    public class CastVoteRequest
    {
        [Required]
        public VoteValue Value { get; set; }  // Pro=1, Contro=2 
    }
}
