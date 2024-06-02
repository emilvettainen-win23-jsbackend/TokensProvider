using Microsoft.EntityFrameworkCore;
using TokensProvider.Infrastructure.Data.Entities;

namespace TokensProvider.Infrastructure.Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
}
