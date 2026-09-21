namespace GymTron.Application.Routines.Queries.DTO;

public class RoutineDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RoutineItemDto> Items { get; set; } = [];
}
