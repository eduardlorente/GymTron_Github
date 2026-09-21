namespace GymTron.Infrastructure.Persistence.DAL.Models;

internal class ExerciseParameterDALModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public int? ReplaysInReserve { get; set; }
}
