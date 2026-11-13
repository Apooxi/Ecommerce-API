using Microsoft.EntityFrameworkCore;

namespace StockManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Models.Product> Products { get; set; }
    public DbSet<Models.User> Users { get; set; }
    
}