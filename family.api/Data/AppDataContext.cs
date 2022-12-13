using family.api.Models;
using Microsoft.EntityFrameworkCore;

namespace family.api.Data;

public class AppDataContext : DbContext
{
    public AppDataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<PageItem> PageItems => Set<PageItem>();
    public DbSet<User> Users => Set<User>();
}