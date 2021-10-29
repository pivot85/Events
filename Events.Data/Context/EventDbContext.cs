namespace Events.Data.Context
{
    using Events.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Implementation of <see cref="DbContext"/> for our Events.
    /// </summary>
    public class EventDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventDbContext"/> class.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions"/> to be injected.</param>
        public EventDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or Sets the <see cref="DbSet{TEntity}"/> containing all events.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="DbSet{TEntity}"/> containing all Permitted Roles.
        /// </summary>
        public DbSet<PermittedRole> PermittedRoles { get; set; }
    }
}