using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Configure Logger
        var name = AppDomain.CurrentDomain.FriendlyName;
        string messageTemplate = 
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        string fileTemplate = $"logs/{name}-.log";
        
        services
            .AddScoped<ILogger, Logger>(_ =>
                new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: messageTemplate)
                    .WriteTo.Async(a => 
                        a.File(
                            Path.Combine(AppContext.BaseDirectory, fileTemplate),
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: messageTemplate,
                            shared: true))
                    .CreateLogger());
        services.AddScoped<Framework.Logger.ILogger, SerilogLogger>();
        
        // Configure Mapper
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.Mapper>(_ => {
            var config = new TypeAdapterConfig();
            return new MapsterMapper.Mapper(config);
        });
        services.AddScoped<Framework.IMapper, MapsterMapperWrapper>();

        return services;
    }
}