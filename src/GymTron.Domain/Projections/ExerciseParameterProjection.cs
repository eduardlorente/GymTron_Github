using GymTron.Domain.Enums;

namespace GymTron.Domain.Projections;

public class ExerciseParameterProjection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public ExerciseTypes Type { get; set; }
    public int? ReplaysInReserve { get; set; }
}
