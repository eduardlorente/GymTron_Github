using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class ExerciseParameters : Entity<int>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Pattern { get; private set; } = string.Empty;
    public int Series { get; private set; }
    public (int Min, int Max) Repetitions { get; private set; }
    public int DurationInSeconds { get; private set; }
    public int? ReplaysInReserve { get; private set; }
    public (int Min, int Max) RestTimeInSeconds { get; private set; }
    public decimal? LastWeight { get; private set; }
    public int? LastDurationInSeconds { get; private set; }
    public int? LastRepetitions { get; private set; }
    public ExerciseTypes Type { get; private set; }

    private readonly List<Observation> _observations = [];
    public IReadOnlyCollection<Observation> Observations => _observations;

    private ExerciseParameters(int id,
                               string name,
                               string description,
                               string pattern,
                               int series,
                               (int Min, int Max) repetitions,
                               int durationInSeconds,
                               int? replaysInReserve,
                               (int Min, int Max) restTimeInSeconds,
                               decimal? lastWeight,
                               int? lastDurationInSeconds,
                               int? lastRepetitions,
                               ExerciseTypes type,
                               List<Observation> observations)
    {
        Id = id;
        Name = name;
        Description = description;
        Pattern = pattern;
        Series = series;
        Repetitions = type == ExerciseTypes.DURATION ? (0, 0) : repetitions;
        DurationInSeconds = type == ExerciseTypes.DURATION ? durationInSeconds : 0;
        ReplaysInReserve = replaysInReserve;
        RestTimeInSeconds = restTimeInSeconds;
        LastWeight = lastWeight;
        LastDurationInSeconds = lastDurationInSeconds;
        LastRepetitions = lastRepetitions;
        Type = type;
        _observations = observations;
    }

    public static ExerciseParameters Create(string name,
                                            string description,
                                            string pattern,
                                            ExerciseTypes type,
                                            int? replaysInReserve = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDomainOperationException("Exercise name cannot be empty.");
        }

        return new(0,
                   name.Trim(),
                   description,
                   pattern,
                   0,
                   (0, 0),
                   0,
                   replaysInReserve,
                   (0, 0),
                   null,
                   null,
                   null,
                   type,
                   []);
    }

    public void Update(string name,
                       string description,
                       string pattern,
                       ExerciseTypes type,
                       int? replaysInReserve)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDomainOperationException("Exercise name cannot be empty.");
        }

        Name = name.Trim();
        Description = description;
        Pattern = pattern;
        Type = type;
        ReplaysInReserve = replaysInReserve;
        if (type == ExerciseTypes.DURATION)
        {
            Repetitions = (0, 0);
        }
        else
        {
            DurationInSeconds = 0;
        }
    }

    public static ExerciseParameters FromDatabase(int id,
                                                  string name,
                                                  string description,
                                                  string pattern,
                                                  int series,
                                                  (int Min, int Max) repetitions,
                                                  int durationInSeconds,
                                                  int? replaysInReserve,
                                                  (int Min, int Max) restTimeInSeconds,
                                                  decimal? lastWeight,
                                                  int? lastDurationInSeconds,
                                                  int? lastRepetitions,
                                                  ExerciseTypes type,
                                                  List<Observation> observations)
    {
        return new ExerciseParameters(id,
                                      name,
                                      description,
                                      pattern,
                                      series,
                                      repetitions,
                                      durationInSeconds,
                                      replaysInReserve,
                                      restTimeInSeconds,
                                      lastWeight,
                                      lastDurationInSeconds,
                                      lastRepetitions,
                                      type,
                                      observations);
    }
}
