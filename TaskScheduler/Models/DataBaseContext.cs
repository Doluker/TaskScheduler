using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TaskScheduler.Models;

public partial class DataBaseContext : DbContext
{
    public DataBaseContext()
    {
    }

    public DataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Priority> Priorities { get; set; }

    public virtual DbSet<Recurrence> Recurrences { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=TaskScheduler;Username=postgres;Password=123;Port=5432");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Priority_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Recurrence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Recurrences_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Schedules_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.DueDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.RecurrenceType).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.RecurrenceTypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Schedules_RecurrenceTypeid_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("Schedules_StatusId_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Schedules_TaskId_fkey");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Statuses_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Tasks_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Priority).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tasks_PriorityId_fkey");

            entity.HasOne(d => d.TaskType).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TaskTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tasks_TaskTypeId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tasks_UserId_fkey");
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TaskType_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.HashPassword).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
