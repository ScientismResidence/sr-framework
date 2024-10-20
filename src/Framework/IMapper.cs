namespace Framework;

public interface IMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
}