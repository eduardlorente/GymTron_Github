namespace GymTron.Domain.ValueObjects;

public class TrainingDate(DateTime date) : ValueObject<TrainingDate>
{


    public DateTime FullDate { get; private set; } = date;


    protected override bool EqualsCore(TrainingDate? other)
    {
        TrainingDate? valueObject = other;

        return FullDate == valueObject?.FullDate;
    }


    protected override int GetHashCodeCore()
    {
        return FullDate.GetHashCode();
    }
}