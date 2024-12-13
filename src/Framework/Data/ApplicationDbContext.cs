using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Framework.Data;

public interface IApplicationDbContext
{
    IContext Context { get; }
}

public class ApplicationDbContext(DbContextOptions options, IContext context)
    : DbContext(options), IApplicationDbContext
{
    public IContext Context { get; } = context;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IMigrationsAssembly, ContextMigrationsAssembly>();
    }
}