using Microsoft.EntityFrameworkCore;
using ZadanieRekrutacyjne_WojciechGanobis.Models;

namespace ZadanieRekrutacyjne_WojciechGanobis.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<TagModel> Tags { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public ApplicationDbContext() { }
}