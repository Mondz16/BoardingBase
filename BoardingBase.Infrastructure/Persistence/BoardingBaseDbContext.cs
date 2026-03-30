

using BoardingBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoardingBase.Infrastructure.Persistence;

public class BoardingBaseDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public BoardingBaseDbContext(DbContextOptions<BoardingBaseDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}