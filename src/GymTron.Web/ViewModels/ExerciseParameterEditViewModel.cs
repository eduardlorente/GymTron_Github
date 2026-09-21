using GymTron.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymTron.Web.ViewModels;

public class ExerciseParameterEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string Pattern { get; set; } = string.Empty;

    [Required]
    public ExerciseTypes Type { get; set; } = ExerciseTypes.WEIGHT;

    [Range(0, 10)]
    public int? ReplaysInReserve { get; set; }
}
