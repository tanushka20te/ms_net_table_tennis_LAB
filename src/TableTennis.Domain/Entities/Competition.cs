using TableTennis.Domain.Enums;

namespace TableTennis.Domain.Entities;

public class Competition : BaseEntity
{
    public Guid OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly RegistrationDeadline { get; set; }

    public DrawType DrawType { get; set; }
    public short BestOf { get; set; } = 5;
    public int? MaxParticipants { get; set; }

    public string? CriteriaParticipants { get; set; }
    public string? RulesGame { get; set; }
    public CompetitionStatus Status { get; set; } = CompetitionStatus.Draft;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}