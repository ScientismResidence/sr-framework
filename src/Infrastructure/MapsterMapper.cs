using Framework;
using IMapsterMapper = MapsterMapper.IMapper;

namespace Infrastructure;

public class MapsterMapper : IMapper
{
    public IMapsterMapper _mapper;

    public MapsterMapper(IMapsterMapper mapper)
    {
        _mapper = mapper;
    }
    
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return _mapper.Map<TSource, TDestination>(source);
    }
}