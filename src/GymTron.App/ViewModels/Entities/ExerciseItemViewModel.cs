namespace GymTron.App.ViewModels.Entities;

public class ExerciseItemViewModel
{

    public int ExerciseParametersId { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsCompleted { get; }
    public bool AlternatingSeries { get; }
    public string TargetInfo { get; }
    public string LastPerformanceInfo { get; }
    public bool HasLastPerformance => !string.IsNullOrWhiteSpace(LastPerformanceInfo);
    public int Position { get; }


    public ExerciseItemViewModel(int exerciseParametersId, string name, bool isCompleted, bool alternatingSeries)
        : this(exerciseParametersId, name, string.Empty, isCompleted, alternatingSeries, string.Empty, string.Empty, 0)
    {
    }


    public ExerciseItemViewModel(int exerciseParametersId, string name, string description, bool isCompleted, bool alternatingSeries)
        : this(exerciseParametersId, name, description, isCompleted, alternatingSeries, string.Empty, string.Empty, 0)
    {
    }

    public ExerciseItemViewModel(
        int exerciseParametersId,
        string name,
        string description,
        bool isCompleted,
        bool alternatingSeries,
        string targetInfo,
        string lastPerformanceInfo,
        int position)
    {
        ExerciseParametersId = exerciseParametersId;
        Name = Helpers.TextEncodingHelper.Sanitize(name);
        Description = Helpers.TextEncodingHelper.Sanitize(description);
        IsCompleted = isCompleted;
        AlternatingSeries = alternatingSeries;
        TargetInfo = targetInfo;
        LastPerformanceInfo = lastPerformanceInfo;
        Position = position;
    }
}
