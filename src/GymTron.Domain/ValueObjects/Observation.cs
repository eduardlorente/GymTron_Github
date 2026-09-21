namespace GymTron.Domain.ValueObjects;

public class Observation(string comment) : ValueObject<Observation>
{


    public string Comment { get; private set; } = comment;


    protected override bool EqualsCore(Observation? other)
    {
        Observation? valueObject = other;

        return Comment == valueObject?.Comment;
    }


    protected override int GetHashCodeCore()
    {
        return Comment.GetHashCode(StringComparison.Ordinal);
    }
}