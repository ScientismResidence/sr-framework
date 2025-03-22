using System.Reflection;
using Framework.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Framework.Data;

#pragma warning disable EF1001
public class ContextMigrationsAssembly : MigrationsAssembly
{
    private readonly IServiceProvider _provider;
    
    public ContextMigrationsAssembly(
        ICurrentDbContext currentContext, 
        IDbContextOptions options, 
        IMigrationsIdGenerator idGenerator, 
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger)
    {
        var dbContext = currentContext.Context as IApplicationDbContext;

        if (dbContext is null)
        {
            throw new FlowException(
                $"{nameof(ContextMigrationsAssembly)} must implement the {nameof(IApplicationDbContext)}");
        }
        
        _provider = dbContext.Provider;
    }
    
    public override Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
    {
        var dependableMigration = migrationClass
            .GetConstructor([typeof(IServiceProvider)]) != null;

        if (dependableMigration)
        {
            var instance = (Migration?)Activator.CreateInstance(migrationClass.AsType(), _provider);

            if (instance is null)
            {
                throw new FlowException(
                    "Unable to create the migration instance, check the related constructor.");
            }
            
            instance.ActiveProvider = activeProvider;
            return instance;
        }

        return base.CreateMigration(migrationClass, activeProvider);
    }
}
#pragma warning restore EF1001