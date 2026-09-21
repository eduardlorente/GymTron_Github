namespace GymTron.Infrastructure.Persistence.DAL.Models;

internal class BodyWeightDALModel
{

    public int Id { get; set; }
    public int? UserId { get; set; }
    public decimal Weight { get; set; }
    public decimal BodyFatPercentage { get; set; }
    public DateTime CreatedOn { get; set; }
}
