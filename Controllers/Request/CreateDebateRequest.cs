using System.ComponentModel.DataAnnotations;

namespace DebatePlatform.Api.Controllers.Request
{
    public class CreateDebateRequest
    {
        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Body { get; set; } = string.Empty;

        //Poi con Identity/JWT dall'utente loggato
        [Required]
        public Guid CreatedByUserId { get; set; }
    }
}
