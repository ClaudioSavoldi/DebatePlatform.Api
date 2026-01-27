using DebatePlatform.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace DebatePlatform.Api.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
      public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options){}
        
        
        public DbSet<User> Users => Set<User>();
        public DbSet<Debate> Debates => Set<Debate>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<DebateStatusHistory> DebateStatusHistories => Set<DebateStatusHistory>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Per ogni voto, considera insieme DebateId e UserIde assicurati che questa coppia sia unica nel database           
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.DebateId, v.UserId })
                .IsUnique();

            // Debate.CreatedBy (evitare cascade delete)
            modelBuilder.Entity<Debate>()
                .HasOne(d => d.CreatedByUser)
                .WithMany(u => u.CreatedDebates)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DebateStatusHistory>()
               .HasOne(h => h.ChangedByUser)
               .WithMany()
               .HasForeignKey(h => h.ChangedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
            
            //gestire parametri Snapshot
            modelBuilder.Entity<DebateStatusHistory>()
                .Property(h => h.ChangedByUsernameSnapshot)
                .HasMaxLength(100);
            modelBuilder.Entity<DebateStatusHistory>()
                .Property(h => h.ChangedByEmailSnapshot)
                .HasMaxLength(254);

        }
    }
}
