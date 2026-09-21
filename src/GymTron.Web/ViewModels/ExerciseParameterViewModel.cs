using GymTron.Domain.Enums;

namespace GymTron.Web.ViewModels;

public class ExerciseParameterViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public ExerciseTypes Type { get; set; }
    public string TypeName => Type switch
    {
        ExerciseTypes.WEIGHT => "Peso",
        ExerciseTypes.DURATION => "Duración",
        _ => "Sin definir"
    };
    public int? ReplaysInReserve { get; set; }
}
