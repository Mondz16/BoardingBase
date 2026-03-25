

using Microsoft.EntityFrameworkCore;

namespace BoardingBase.Infrastructure.Persistence;

public class BoardingBaseDbContext : DbContext
{
    public BoardingBaseDbContext(DbContextOptions<BoardingBaseDbContext> options) : base(options)
    {
        
    }

    
}