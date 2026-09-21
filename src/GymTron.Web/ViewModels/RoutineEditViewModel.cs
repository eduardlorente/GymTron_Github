using System.ComponentModel.DataAnnotations;
using GymTron.Domain.Enums;

namespace GymTron.Web.ViewModels;

public class RoutineEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    public List<RoutineItemEditViewModel> Items { get; set; } = [];
}

public class RoutineItemEditViewModel
{
    public int Id { get; set; }

    [Required]
    [Range(1, 7, ErrorMessage = "El día debe estar entre 1 y 7")]
    public int DayOfWeek { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un ejercicio")]
    public int ExerciseParametersId { get; set; }
    
    public string ExerciseName { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Series { get; set; } = 3;

    public int RepetitionsMin { get; set; } = 8;

    public int RepetitionsMax { get; set; } = 12;

    public int? Duration { get; set; }

    [Range(0, 600)]
    public int MinRestTimeInSeconds { get; set; } = 60;

    [Range(0, 600)]
    public int? MaxRestTimeInSeconds { get; set; } = 90;

    public bool AlternatingSeries { get; set; }

    public int Position { get; set; } = 1;
    
    public ExerciseTypes Type { get; set; }
}
