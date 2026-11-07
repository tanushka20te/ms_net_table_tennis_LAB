namespace TableTennis.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

// Фиксированные значения справочника ролей
public static class RoleIds
{
    public const int Sportsman = 1;
    public const int Coach = 2;
    public const int Organizer = 3;
}
