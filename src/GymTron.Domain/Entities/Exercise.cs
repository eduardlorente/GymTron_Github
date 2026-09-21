using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class Exercise : Entity<int>
{


    public int TrainingId { get; private set; }
    public int ExerciseParametersId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Weight { get; private set; } = 0;
    public int DurationInSeconds { get; private set; } = 0;
    public int CurrentRepetitions { get; private set; } = 0;
    public List<Observation> Observations { get; private set; } = [];


    private Exercise(int trainingId,
                     int exerciseParametersId,
                     string name,
                     decimal weight,
                     int duration,
                     int repetitions,
                     DateTime createdOn,
                     List<string> observations)
        : base(0)
    {
        TrainingId = trainingId;
        ExerciseParametersId = exerciseParametersId;
        Name = name;
        Weight = weight;
        DurationInSeconds = duration;
        CurrentRepetitions = repetitions;
        Status = EntityStatus.FromDatabase(createdOn);
        Observations = observations.Select(o => new Observation(o)).ToList();
    }


    private Exercise(int id,
                     int trainingId,
                     int exerciseParametersId,
                     string name,
                     decimal weight,
                     int duration,
                     int repetitions,
                     DateTime createdOn,
                     List<string> observations)
        : base(id)
    {
        TrainingId = trainingId;
        ExerciseParametersId = exerciseParametersId;
        Name = name;
        Weight = weight;
        DurationInSeconds = duration;
        CurrentRepetitions = repetitions;
        Status = EntityStatus.FromDatabase(createdOn);
        Observations = observations.Select(o => new Observation(o)).ToList();
    }


    public static Exercise New(int trainingId,
                               int exerciseParametersId,
                               string name,
                               decimal weight,
                               int duration,
                               int repetitions,
                               List<string> observations,
                               IClock clock)
    {
        return new Exercise(trainingId, exerciseParametersId, name, weight, duration, repetitions, clock.UtcNow, observations);
    }


    public static Exercise New(int trainingId,
                               int exerciseParametersId,
                               string name,
                               decimal weight,
                               int duration,
                               int repetitions,
                               DateTime createdOn,
                               List<string> observations)
    {
        return new Exercise(trainingId, exerciseParametersId, name, weight, duration, repetitions, createdOn, observations);
    }


    public static Exercise New(int trainingId,
                               int exerciseParametersId,
                               string name,
                               decimal weight,
                               int duration,
                               int repetitions,
                               List<string> observations,
                               DateTime createdOn)
    {
        return new Exercise(trainingId, exerciseParametersId, name, weight, duration, repetitions, createdOn, observations);
    }


    public static Exercise New(int trainingId,
                               int exerciseParametersId,
                               string name,
                               decimal weight,
                               int duration,
                               int repetitions,
                               List<string> observations)
    {
        return new Exercise(trainingId, exerciseParametersId, name, weight, duration, repetitions, default, observations);
    }


    public static Exercise FromDatabase(int id,
                                        int trainingId,
                                        int exerciseParametersId,
                                        string name,
                                        decimal weight,
                                        int duration,
                                        int repetitions,
                                        DateTime createdOn, 
                                        List<string> observations)
    {
        return new Exercise(id, trainingId, exerciseParametersId, name, weight, duration, repetitions, createdOn, observations);
    }
}
