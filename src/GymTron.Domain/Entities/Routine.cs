using GymTron.Domain.Exceptions;

namespace GymTron.Domain.Entities;

public class Routine : Entity<int>
{
    public int? UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    private readonly List<RoutineItem> _routineExercises = [];
    public IReadOnlyCollection<RoutineItem> RoutineExercises => _routineExercises;

    public Dictionary<int, List<RoutineItem>> WorkByDays
    {
        get
        {
            return _routineExercises
                .GroupBy(s => s.DayOfWeek)
                .ToDictionary(s => s.Key, s => s.ToList());
        }
    }

    private Routine(int id,
                    int? userId,
                    string name,
                    List<RoutineItem> routineExercises)
        : base(id)
    {
        UserId = userId;
        Name = name;
        _routineExercises = routineExercises;
    }

    public static Routine Create(string name, List<RoutineItem> routineExercises, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDomainOperationException("Routine name cannot be empty.");
        }

        return new(0, userId, name.Trim(), routineExercises);
    }

    public static Routine FromDatabase(int id,
                                       string name,
                                       List<RoutineItem> routineExercises,
                                       int? userId = null)
    {
        return new(id, userId, name, routineExercises);
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new InvalidDomainOperationException("Routine name cannot be empty.");
        }

        Name = newName.Trim();
    }
}
