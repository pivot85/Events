using Events.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Data.Context
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions options)
            : base(options)
        {
        }
        
        public DbSet<Event> Events { get; set; }
        public DbSet<PermittedRole> PermittedRoles { get; set; }
    }
}