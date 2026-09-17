using AgenticEngineeringSystem.Core.Governance;
using AgenticEngineeringSystem.Core.Orchestration;
using AgenticEngineeringSystem.Core.UrlShortener;
using Microsoft.EntityFrameworkCore;

namespace AgenticEngineeringSystem.Infrastructure.Data;

public sealed class AgenticEngineeringDbContext(DbContextOptions<AgenticEngineeringDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<UrlVisit> UrlVisits => Set<UrlVisit>();

    public DbSet<EngineeringWorkflow> Workflows => Set<EngineeringWorkflow>();

    public DbSet<EngineeringTask> EngineeringTasks => Set<EngineeringTask>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(url => url.Id);
            entity.HasIndex(url => url.ShortCode).IsUnique();
            entity.Property(url => url.ShortCode).HasMaxLength(32).IsRequired();
            entity.Property(url => url.OriginalUrl).HasMaxLength(2048).IsRequired();
            entity.HasMany(url => url.Visits)
                .WithOne(visit => visit.ShortUrl)
                .HasForeignKey(visit => visit.ShortUrlId);
        });

        modelBuilder.Entity<UrlVisit>(entity =>
        {
            entity.HasKey(visit => visit.Id);
            entity.Property(visit => visit.IpAddress).HasMaxLength(64);
            entity.Property(visit => visit.UserAgent).HasMaxLength(512);
            entity.Property(visit => visit.Referrer).HasMaxLength(2048);
        });

        modelBuilder.Entity<EngineeringWorkflow>(entity =>
        {
            entity.HasKey(workflow => workflow.Id);
            entity.Property(workflow => workflow.Name).HasMaxLength(160).IsRequired();
            entity.Property(workflow => workflow.State).HasConversion<string>().HasMaxLength(40);
            entity.HasMany(workflow => workflow.Tasks)
                .WithOne(task => task.Workflow)
                .HasForeignKey(task => task.WorkflowId);
        });

        modelBuilder.Entity<EngineeringTask>(entity =>
        {
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Name).HasMaxLength(160).IsRequired();
            entity.Property(task => task.Agent).HasMaxLength(80).IsRequired();
            entity.Property(task => task.Status).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<EngineeringTaskDependency>(entity =>
        {
            entity.HasKey(dependency => new { dependency.TaskId, dependency.DependsOnTaskId });
            entity.HasOne(dependency => dependency.Task)
                .WithMany(task => task.Dependencies)
                .HasForeignKey(dependency => dependency.TaskId);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasKey(auditEvent => auditEvent.Id);
            entity.Property(auditEvent => auditEvent.Actor).HasMaxLength(120).IsRequired();
            entity.Property(auditEvent => auditEvent.Action).HasMaxLength(120).IsRequired();
            entity.Property(auditEvent => auditEvent.Target).HasMaxLength(240).IsRequired();
            entity.Property(auditEvent => auditEvent.Result).HasMaxLength(80).IsRequired();
            entity.Property(auditEvent => auditEvent.Reason).HasMaxLength(1024);
        });
    }
}
