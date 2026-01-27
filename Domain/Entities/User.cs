namespace DebatePlatform.Api.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<Debate> CreatedDebates { get; set; } = new List<Debate>();
    }
}
