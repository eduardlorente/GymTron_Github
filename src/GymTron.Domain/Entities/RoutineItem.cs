using GymTron.Domain.Exceptions;

namespace GymTron.Domain.Entities;

public class RoutineItem : Entity<int>
{
    public int DayOfWeek { get; private set; }
    public ExerciseParameters ExerciseParameters { get; private set; }
    public bool AlternatingSeries { get; private set; } = false;
    public int Position { get; private set; }

    private RoutineItem(int id,
                        int dayOfWeek,
                        ExerciseParameters exerciseParameters,
                        bool alternatingSeries,
                        int position)
    {
        Id = id;
        DayOfWeek = dayOfWeek;
        ExerciseParameters = exerciseParameters;
        AlternatingSeries = alternatingSeries;
        Position = position;
    }

    public static RoutineItem Create(int dayOfWeek,
                                     ExerciseParameters exerciseParameters,
                                     bool alternatingSeries,
                                     int position)
    {
        if (dayOfWeek < 1 || dayOfWeek > 7)
        {
            throw new InvalidDomainOperationException("Day of week must be between 1 and 7.");
        }

        ArgumentNullException.ThrowIfNull(exerciseParameters);

        return new(0,
                   dayOfWeek,
                   exerciseParameters,
                   alternatingSeries,
                   position);
    }

    public static RoutineItem FromDatabase(int id,
                                           int dayOfWeek,
                                           ExerciseParameters exerciseParameters,
                                           bool alternatingSeries,
                                           int position)
    {
        return new(id,
                   dayOfWeek,
                   exerciseParameters,
                   alternatingSeries,
                   position);
    }
}
