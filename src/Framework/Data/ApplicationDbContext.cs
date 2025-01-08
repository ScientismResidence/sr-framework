using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Framework.Data;

public interface IApplicationDbContext
{
    IServiceProvider Provider { get; }
}

public class ApplicationDbContext(DbContextOptions options, IServiceProvider provider)
    : DbContext(options), IApplicationDbContext
{
    public IServiceProvider Provider { get; } = provider;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IMigrationsAssembly, ContextMigrationsAssembly>();
    }
}