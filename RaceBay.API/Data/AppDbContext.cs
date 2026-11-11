using Microsoft.EntityFrameworkCore;
using RaceBay.API.Models;
using System.Data.Entity;

namespace RaceBay.API.Data;

public class AppDbContext : DbContext
{

    public DbSet<User> Users { get; set; }
}