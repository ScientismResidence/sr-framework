using Framework;
using IMapsterMapper = MapsterMapper.IMapper;

namespace Infrastructure;

public class MapsterMapperWrapper(IMapsterMapper mapper) : IMapper
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return mapper.Map<TSource, TDestination>(source);
    }
}