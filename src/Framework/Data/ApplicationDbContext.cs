using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Framework.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options, IContext context)
        : base(options)
    {
        Context = context;
    }
    
    public IContext Context { get; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IMigrationsAssembly, ContextMigrationsAssembly>();
    }
}