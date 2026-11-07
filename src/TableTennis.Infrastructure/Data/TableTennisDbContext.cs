using Microsoft.EntityFrameworkCore;
using TableTennis.Domain.Entities;

namespace TableTennis.Infrastructure.Data;

public class TableTennisDbContext : DbContext
{
    public TableTennisDbContext(DbContextOptions<TableTennisDbContext> options)
        : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Roles ----------
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.Property(r => r.Name).HasMaxLength(30).IsRequired();
            e.HasIndex(r => r.Name).IsUnique();

            e.HasData(
                new Role { Id = RoleIds.Sportsman, Name = "Sportsman" },
                new Role { Id = RoleIds.Coach, Name = "Coach" },
                new Role { Id = RoleIds.Organizer, Name = "Organizer" });
        });

        // ---------- Users ----------
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users", t =>
            {
                // Тренеру и организатору email обязателен, спортсмену нет
                t.HasCheckConstraint("CK_Users_EmailRequired",
                    $"\"RoleId\" = {RoleIds.Sportsman} OR \"Email\" IS NOT NULL");
                // Тренер указывается только у спортсмена
                t.HasCheckConstraint("CK_Users_CoachOnlyForSportsman",
                    $"\"RoleId\" = {RoleIds.Sportsman} OR \"CoachId\" IS NULL");
                // Нельзя быть тренером самому себе
                t.HasCheckConstraint("CK_Users_NotOwnCoach",
                    "\"CoachId\" IS NULL OR \"CoachId\" <> \"Id\"");
            });

            e.Property(u => u.Surname).HasMaxLength(100).IsRequired();
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Patronymic).HasMaxLength(100);
            e.Property(u => u.City).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(200);
            e.Property(u => u.Nickname).HasMaxLength(200);
            e.Property(u => u.Category).HasMaxLength(100);
            e.Property(u => u.InviteCode).HasMaxLength(20);
            e.Property(u => u.Gender).HasConversion<string>().HasMaxLength(10);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            // В PostgreSQL несколько NULL в уникальном индексе допускаются
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.InviteCode).IsUnique();

            // Role 1:M Users
            e.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Рекурсивная связь: тренер 1:M спортсмены
            e.HasOne(u => u.Coach)
                .WithMany(u => u.Athletes)
                .HasForeignKey(u => u.CoachId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- Competitions ----------
        modelBuilder.Entity<Competition>(e =>
        {
            e.ToTable("Competitions", t =>
            {
                t.HasCheckConstraint("CK_Competitions_Dates", "\"EndDate\" >= \"StartDate\"");
                t.HasCheckConstraint("CK_Competitions_BestOf", "\"BestOf\" IN (3, 5, 7)");
            });

            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Place).HasMaxLength(200).IsRequired();
            e.Property(c => c.DrawType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

            // Users (организатор) 1:M Competitions
            e.HasOne(c => c.Organizer)
                .WithMany(u => u.OrganizedCompetitions)
                .HasForeignKey(c => c.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Registrations (M:N Users <-> Competitions) ----------
        modelBuilder.Entity<Registration>(e =>
        {
            e.ToTable("Registrations");

            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.SubmittedAt).HasDefaultValueSql("now()");

            // Один спортсмен не может подать две заявки на один турнир
            e.HasIndex(r => new { r.CompetitionId, r.SportsmanId }).IsUnique();

            e.HasOne(r => r.Competition)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Sportsman)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.SportsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Coach)
                .WithMany(u => u.SubmittedRegistrations)
                .HasForeignKey(r => r.CoachId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
