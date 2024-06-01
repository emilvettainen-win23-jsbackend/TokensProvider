using Microsoft.EntityFrameworkCore;
using TokensProvider.Infrastructure.Data.Entities;

namespace TokensProvider.Infrastructure.Data.Contexts;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
}
