using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.ValueObjects.Polls;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.ToTable("polls");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                  .HasConversion(
                      v => v.Value,
                      v => PollId.Create(v))
                  .HasColumnName("id");

            entity.Property(p => p.Question)
                  .HasConversion(
                      v => v.Value,
                      v => QuestionText.Create(v))
                  .HasColumnName("question")
                  .HasMaxLength(500);

            entity.Property(p => p.IsActive)
                  .HasColumnName("is_active");

            entity.HasMany(p => p.Options)
                  .WithOne()
                  .HasForeignKey("poll_id")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Votes)
                  .WithOne()
                  .HasForeignKey("poll_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PollOption>(entity =>
        {
            entity.ToTable("poll_options");

            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id)
                  .HasConversion(
                      v => v.Value,
                      v => PollOptionId.Create(v))
                  .HasColumnName("id");

            entity.Property(o => o.Text)
                  .HasConversion(
                      v => v.Value,
                      v => OptionText.Create(v))
                  .HasColumnName("text")
                  .HasMaxLength(300);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("votes");

            entity.HasKey(v => new { VoterId = v.VoterId, OptionId = v.OptionId });

            entity.Property(v => v.VoterId)
                  .HasConversion(
                      v => v.Value,
                      v => VoterId.Create(v))
                  .HasColumnName("voter_id")
                  .HasMaxLength(200);

            entity.Property(v => v.OptionId)
                  .HasConversion(
                      v => v.Value,
                      v => PollOptionId.Create(v))
                  .HasColumnName("option_id");

            entity.Property(v => v.CreatedAt)
                  .HasColumnName("created_at");
        });
    }
}
