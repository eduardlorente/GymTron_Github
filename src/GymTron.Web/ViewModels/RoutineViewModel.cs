namespace GymTron.Web.ViewModels;

public class RoutineViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RoutineItemViewModel> Items { get; set; } = [];
    public Dictionary<int, List<RoutineItemViewModel>> WorkByDays => 
        Items.GroupBy(x => x.DayOfWeek)
             .ToDictionary(x => x.Key, x => x.ToList());
}

public class RoutineItemViewModel
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName => DayOfWeek switch
    {
        1 => "Lunes",
        2 => "Martes",
        3 => "Miércoles",
        4 => "Jueves",
        5 => "Viernes",
        6 => "Sábado",
        7 => "Domingo",
        _ => $"Día {DayOfWeek}"
    };
    public int ExerciseParametersId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Series { get; set; }
    public int RepetitionsMin { get; set; }
    public int RepetitionsMax { get; set; }
    public int MinRestTimeInSeconds { get; set; }
    public int? MaxRestTimeInSeconds { get; set; }
    public bool AlternatingSeries { get; set; }
}
