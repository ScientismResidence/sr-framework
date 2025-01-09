using Mapster;
using Serilog;
using Serilog.Core;

namespace Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    private const string MessageTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Configure Logger
        services.AddScoped<Framework.Logger.ILogger, SerilogLogger>(_ => new SerilogLogger(GetLogger()));
        services.AddSingleton<Framework.Logger.IRootLogger, SerilogLogger>(_ => new SerilogLogger(GetLogger()));
        
        // Configure Mapper
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.Mapper>(_ => {
            var config = new TypeAdapterConfig();
            return new MapsterMapper.Mapper(config);
        });
        services.AddScoped<Framework.IMapper, MapsterMapperWrapper>();

        return services;
    }

    private static Logger GetLogger()
    {
        string fileTemplate = $"logs/{AppDomain.CurrentDomain.FriendlyName}-.log";
        
        return new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: MessageTemplate)
            .WriteTo.Async(a =>
                a.File(
                    Path.Combine(AppContext.BaseDirectory, fileTemplate),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: MessageTemplate,
                    shared: true))
            .CreateLogger();
    }
}