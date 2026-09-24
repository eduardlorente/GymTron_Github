namespace GymTron.Infrastructure.Persistence.DAL.Models;

internal record TrainingHistoryDALModel
{
    public DateTime StartedOn { get; init; }
    public int DayOfWeek { get; init; }

    public TrainingHistoryDALModel() { }

    public TrainingHistoryDALModel(DateTime startedOn, int dayOfWeek)
    {
        StartedOn = startedOn;
        DayOfWeek = dayOfWeek;
    }
}
