using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Framework.Data;

public interface IDbTransactionContext
{
    Task<IDbContextTransaction> BeginTransaction(CancellationToken token);
}

public class DbTransactionContext<TDbContext>(TDbContext context) : IDbTransactionContext
    where TDbContext : DbContext
{
    public async Task<IDbContextTransaction> BeginTransaction(CancellationToken token)
    {
        return await context.Database.BeginTransactionAsync(token);
    }
}