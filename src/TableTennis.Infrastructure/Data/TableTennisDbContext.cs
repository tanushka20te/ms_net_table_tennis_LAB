using Microsoft.EntityFrameworkCore;
using TableTennis.Domain.Entities;

namespace TableTennis.Infrastructure.Data;

public class TableTennisDbContext : DbContext
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Registration> Registrations => Set<Registration>();

    public TableTennisDbContext(DbContextOptions<TableTennisDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- roles ----------
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Name).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();

            // Начальные данные: у HasData значения должны быть постоянными
            e.HasData(
                new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Sportsman" },
                new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Coach" },
                new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Organizer" });
        });

        // ---------- users ----------
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users", t =>
            {
                // Нельзя быть тренером самому себе
                t.HasCheckConstraint("CK_users_not_own_coach",
                    "\"CoachId\" IS NULL OR \"CoachId\" <> \"Id\"");
            });

            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Surname).HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Patronymic).HasMaxLength(100);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Nickname).HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.InviteCode).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            // Gender хранится числом: 0 (Female) или 1 (Male)

            // Уникальные значения (в PostgreSQL несколько NULL допускаются)
            e.HasIndex(x => x.Nickname).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.InviteCode).IsUnique();

            // roles 1:M users
            e.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Рекурсивная связь: тренер 1:M спортсмены
            e.HasOne(x => x.Coach)
                .WithMany(x => x.Athletes)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ---------- competitions ----------
        modelBuilder.Entity<Competition>(e =>
        {
            e.ToTable("competitions", t =>
            {
                t.HasCheckConstraint("CK_competitions_dates", "\"EndDate\" >= \"StartDate\"");
                t.HasCheckConstraint("CK_competitions_best_of", "\"BestOf\" IN (3, 5, 7)");
            });

            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Place).HasMaxLength(200).IsRequired();
            e.Property(x => x.DrawType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            // users (организатор) 1:M competitions
            e.HasOne(x => x.Organizer)
                .WithMany(x => x.OrganizedCompetitions)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- registrations (M:N users <-> competitions) ----------
        modelBuilder.Entity<Registration>(e =>
        {
            e.ToTable("registrations");

            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SubmittedAt).HasDefaultValueSql("now()");

            // Один спортсмен не может подать две заявки на один турнир
            e.HasIndex(x => new { x.CompetitionId, x.SportsmanId }).IsUnique();

            e.HasOne(x => x.Competition)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.CompetitionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Sportsman)
                .WithMany(x => x.Registrations)
                .HasForeignKey(x => x.SportsmanId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Coach)
                .WithMany(x => x.SubmittedRegistrations)
                .HasForeignKey(x => x.CoachId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}