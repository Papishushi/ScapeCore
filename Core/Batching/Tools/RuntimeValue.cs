namespace ScapeCore.Core.Batching.Tools
{

    public sealed class RuntimeValue<T> : IRuntimeValue<T> where T : struct
    {
        public RuntimeValue(T value)
        {
            _value=value;
        }

        public static implicit operator T(RuntimeValue<T> runtimeValue) => runtimeValue._value;
        public static implicit operator RuntimeValue<T>(T value) => new(value);

        public T Value { get => _value; set => _value = value; }
        private T _value;

        public override bool Equals(object obj)
        {
            if (obj is RuntimeValue<T> temp)
            {
                if (temp._value.Equals(_value))
                    return true;
                else
                    return false;
            }
            if (obj == null)
                return true;
            else
                return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString() => _value.ToString();
    }
}