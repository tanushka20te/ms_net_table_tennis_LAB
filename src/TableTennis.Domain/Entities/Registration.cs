using TableTennis.Domain.Enums;

namespace TableTennis.Domain.Entities;

// Связующая сущность M:N между спортсменами (Users) и турнирами (Competition)
public class Registration
{
    public Guid Id { get; set; }

    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public Guid SportsmanId { get; set; }
    public User Sportsman { get; set; } = null!;

    // Кто подал заявку (тренер), может быть пустым
    public Guid? CoachId { get; set; }
    public User? Coach { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public int? Seed { get; set; }
    public DateTime SubmittedAt { get; set; }
}
