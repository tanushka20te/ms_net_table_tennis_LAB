using TableTennis.Domain.Enums;

namespace TableTennis.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    // Рекурсивная связь: тренер и его спортсмены хранятся в одной таблице
    public Guid? CoachId { get; set; }
    public User? Coach { get; set; }
    public ICollection<User> Athletes { get; set; } = new List<User>();

    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? City { get; set; }

    public string? Email { get; set; }
    public string? Nickname { get; set; }

    public int? Rating { get; set; }          // только у спортсменов
    public string? Category { get; set; }     // разряд
    public string? InviteCode { get; set; }   // чтобы спортсмен "забрал" профиль
    public DateTime CreatedAt { get; set; }

    // Турниры, которые создал этот пользователь как организатор
    public ICollection<Competition> OrganizedCompetitions { get; set; } = new List<Competition>();
    // Заявки, где пользователь выступает спортсменом
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    // Заявки, которые подал этот пользователь как тренер
    public ICollection<Registration> SubmittedRegistrations { get; set; } = new List<Registration>();
}
