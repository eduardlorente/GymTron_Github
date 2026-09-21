namespace GymTron.Domain.Projections;

public class BodyWeightHistoryProjection
{
    public DateTime CreatedOn { get; set; }
    public decimal Weight { get; set; }
    public decimal BodyFatPercentage { get; set; }
}
