using System.ComponentModel.DataAnnotations;

namespace DebatePlatform.Api.Controllers.Request
{
    public class SubmitOpeningRequest
    {
        [Required]
        [MaxLength(5000)]
        public string Body { get; set; } = string.Empty;
    }
}
