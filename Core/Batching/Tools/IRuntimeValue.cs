namespace ScapeCore.Core.Batching.Tools
{
    public interface IRuntimeValue<T> where T : struct
    {
        T Value { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        string ToString();
    }
}