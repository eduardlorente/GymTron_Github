namespace GymTron.Domain.ValueObjects;

public abstract class ValueObject<T> : IEquatable<T> where T : ValueObject<T>
{


    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (T)obj;
        return EqualsCore(other);
    }


    public bool Equals(T? other)
    {
        return EqualsCore(other);
    }


    public override int GetHashCode()
    {
        return GetHashCodeCore();
    }


    protected abstract bool EqualsCore(T? other);


    protected abstract int GetHashCodeCore();


    public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
    {
        return Equals(left, right);
    }


    public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
    {
        return !Equals(left, right);
    }
}
