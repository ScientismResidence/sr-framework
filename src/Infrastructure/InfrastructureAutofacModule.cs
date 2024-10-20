using Autofac;
using Framework;
using Mapster;
using Serilog;
using ILogger = Framework.Logger.ILogger;
using IMapper = Framework.IMapper;
using Mapper = Infrastructure.MapsterMapper;
using INativeMapper = MapsterMapper.IMapper;
using NativeMapper = MapsterMapper.Mapper;

namespace Infrastructure;

public class InfrastructureAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Configure Context
        builder.RegisterType<AutofacContext>().As<IContext>().InstancePerLifetimeScope();
        
        // Configure Logger
        var name = AppDomain.CurrentDomain.FriendlyName;
        string messageTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        string fileTemplate = $"logs/{name}-.log";
        
        builder
            .Register(_ =>
                new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: messageTemplate)
                    .WriteTo.Async(a => 
                        a.File(
                            Path.Combine(AppContext.BaseDirectory, fileTemplate),
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: messageTemplate,
                            shared: true))
                    .CreateLogger())
            .As<Serilog.ILogger>()
            .OwnedByLifetimeScope();
        builder.RegisterType<SerilogLogger>().As<ILogger>().OwnedByLifetimeScope();
        
        // Configure Mapper
        builder.Register(_ => {
            var config = new TypeAdapterConfig();
            return new NativeMapper(config);
        }).As<INativeMapper>().InstancePerLifetimeScope();
        builder.RegisterType<Mapper>().As<IMapper>().InstancePerLifetimeScope();
    }
}