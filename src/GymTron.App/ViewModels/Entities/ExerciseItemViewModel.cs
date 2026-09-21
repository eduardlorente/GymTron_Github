namespace GymTron.App.ViewModels.Entities;

public class ExerciseItemViewModel
{

    public int ExerciseParametersId { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsCompleted { get; }
    public bool AlternatingSeries { get; }


    public ExerciseItemViewModel(int exerciseParametersId, string name, bool isCompleted, bool alternatingSeries)
    {
        ExerciseParametersId = exerciseParametersId;
        Name = name;
        IsCompleted = isCompleted;
        AlternatingSeries = alternatingSeries;
    }


    public ExerciseItemViewModel(int exerciseParametersId, string name, string description, bool isCompleted, bool alternatingSeries)
    {
        ExerciseParametersId = exerciseParametersId;
        Name = name;
        Description = description;
        IsCompleted = isCompleted;
        AlternatingSeries = alternatingSeries;
    }
}
