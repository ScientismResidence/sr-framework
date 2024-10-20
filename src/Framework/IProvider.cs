namespace Framework;

public interface IProvider<TKey> : IReadOnlyDictionary<TKey, string> where TKey : struct, Enum
{
    
}