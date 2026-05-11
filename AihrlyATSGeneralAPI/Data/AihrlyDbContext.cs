using AihrlyATSGeneralAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AihrlyATSGeneralAPI.Data;

public class AihrlyDbContext : DbContext
{
    public AihrlyDbContext(DbContextOptions<AihrlyDbContext> options) : base(options) { }

    public DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationNote> ApplicationNotes { get; set; } = null!;
    public DbSet<StageHistory> StageHistories { get; set; } = null!;
    public DbSet<CultureFitScore> CultureFitScores { get; set; } = null!;
    public DbSet<InterviewScore> InterviewScores { get; set; } = null!;
    public DbSet<AssessmentScore> AssessmentScores { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Duplicate application rule: same email + same job -> rejected
        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.JobId, a.CandidateEmail })
            .IsUnique();

        // 1-to-1 relationships for scores
        modelBuilder.Entity<Application>()
            .HasOne(a => a.CultureFitScore)
            .WithOne(s => s.Application)
            .HasForeignKey<CultureFitScore>(s => s.ApplicationId);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.InterviewScore)
            .WithOne(s => s.Application)
            .HasForeignKey<InterviewScore>(s => s.ApplicationId);

        modelBuilder.Entity<Application>()
            .HasOne(a => a.AssessmentScore)
            .WithOne(s => s.Application)
            .HasForeignKey<AssessmentScore>(s => s.ApplicationId);

        // Configure UpdatedBy/CreatedBy relationships
        modelBuilder.Entity<CultureFitScore>()
            .HasOne(s => s.UpdatedBy)
            .WithMany()
            .HasForeignKey(s => s.UpdatedById);

        modelBuilder.Entity<InterviewScore>()
            .HasOne(s => s.UpdatedBy)
            .WithMany()
            .HasForeignKey(s => s.UpdatedById);

        modelBuilder.Entity<AssessmentScore>()
            .HasOne(s => s.UpdatedBy)
            .WithMany()
            .HasForeignKey(s => s.UpdatedById);

        modelBuilder.Entity<ApplicationNote>()
            .HasOne(n => n.CreatedBy)
            .WithMany()
            .HasForeignKey(n => n.CreatedById);

        modelBuilder.Entity<StageHistory>()
            .HasOne(h => h.ChangedBy)
            .WithMany()
            .HasForeignKey(h => h.ChangedById);
    }
}
