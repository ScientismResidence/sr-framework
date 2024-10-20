namespace Framework
{
    public interface IContext
    {
        T Get<T>();

        object Get(Type type);
    }
}
