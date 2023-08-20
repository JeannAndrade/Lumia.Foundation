namespace LumiaFoundation.Domain
{
    public class GenericSingleValueObject<T> : ValueObject
    {
        public T Value { get; private set; }

        public GenericSingleValueObject(T value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value), "Value must not be null");

            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
#pragma warning disable CS8603 // Rethrow to preserve stack details
            yield return Value;
#pragma warning restore CS8603 // Rethrow to preserve stack details
        }

        public static implicit operator GenericSingleValueObject<T>(T value)
        {
            return new GenericSingleValueObject<T>(value);
        }

        public static implicit operator T(GenericSingleValueObject<T> value)
        {
            return value.Value;
        }
    }
}