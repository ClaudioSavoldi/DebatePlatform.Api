using DebatePlatform.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace DebatePlatform.Api.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
      public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options){}
        
        
        public DbSet<Debate> Debates => Set<Debate>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<DebateStatusHistory> DebateStatusHistories => Set<DebateStatusHistory>();
        public DbSet<DebateMatch> DebateMatches => Set<DebateMatch>();
        public DbSet<DebateQueueEntry> DebateQueueEntries => Set<DebateQueueEntry>();
        public DbSet<MatchSubmission> MatchSubmissions => Set<MatchSubmission>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Per ogni voto, considera insieme MatchId e UserIde        
            modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.MatchId, v.UserId })
            .IsUnique();



            //gestire parametri Snapshot
            modelBuilder.Entity<DebateStatusHistory>()
                .Property(h => h.ChangedByUsernameSnapshot)
                .HasMaxLength(100);
            modelBuilder.Entity<DebateStatusHistory>()
                .Property(h => h.ChangedByEmailSnapshot)
                .HasMaxLength(254);

            // Un utente può stare in coda UNA volta per topic
            modelBuilder.Entity<DebateQueueEntry>()
                .HasIndex(q => new { q.DebateId, q.UserId })
                .IsUnique();

            // Ordinamento coda 
            modelBuilder.Entity<DebateQueueEntry>()
                .HasIndex(q => new { q.DebateId, q.Side, q.JoinedAt });

            // Una submission per (match, user, phase)
            modelBuilder.Entity<MatchSubmission>()
                .HasIndex(s => new { s.MatchId, s.UserId, s.Phase })
                .IsUnique();



        }
    }
}
