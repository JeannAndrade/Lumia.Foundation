namespace LumiaFoundation.Domain
{
  public abstract class ValueObject
  {
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
      if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
        return true;

      if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        return false;

      return left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
      return !EqualOperator(left, right);
    }

    protected abstract IEnumerable<object> GetEqualityComponents();


    public override bool Equals(object? obj)
    {
      var compareTo = obj as ValueObject;

      if (ReferenceEquals(this, compareTo)) return true;
      if (compareTo is null) return false;

      return GetEqualityComponents().SequenceEqual(compareTo.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
      return GetEqualityComponents()
          .Select(x => x != null ? x.GetHashCode() : 0)
          .Aggregate((x, y) => x ^ y);
    }

    // Other utility methods
    public static bool operator ==(ValueObject one, ValueObject two)
    {
      return EqualOperator(one, two);
    }

    public static bool operator !=(ValueObject one, ValueObject two)
    {
      return NotEqualOperator(one, two);
    }
  }
}