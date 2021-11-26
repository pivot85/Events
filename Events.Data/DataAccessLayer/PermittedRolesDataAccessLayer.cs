namespace Events.Data.DataAccessLayer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Events.Data.Context;
    using Events.Data.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The Data Access Layer for the Permitted Roles Table.
    /// </summary>
    public class PermittedRolesDataAccessLayer : IPermittedRolesDataAccessLayer
    {
        private readonly IDbContextFactory<EventsDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermittedRolesDataAccessLayer"/> class.
        /// </summary>
        /// <param name="contextFactory">The <see cref="IDbContextFactory{TContext}"/> to be injected.</param>
        public PermittedRolesDataAccessLayer(IDbContextFactory<EventsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get a single permitted role.
        /// </summary>
        /// <param name="guildId">The guild which the role belongs.</param>
        /// <param name="permittedRole">The role being searched for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<PermittedRole> GetByGuild(ulong guildId, ulong permittedRole)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PermittedRoles
                .Where(x => x.GuildId == guildId && x.Id == permittedRole)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get a list of all permitted roles for a guild.
        /// </summary>
        /// <param name="guildId">The guild which the roles belong.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<IEnumerable<PermittedRole>> GetAllByGuild(ulong guildId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PermittedRoles
                .ToListAsync();
        }

        /// <summary>
        /// Create a permitted role.
        /// </summary>
        /// <param name="guildId">The guild which the role belongs.</param>
        /// <param name="permittedRoleId">The role to be added.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Create(ulong guildId, ulong permittedRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Add(new PermittedRole
            {
                Id = permittedRoleId,
                GuildId = guildId,
            });

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a Permitted Role.
        /// </summary>
        /// <param name="guildId">The guild in which the role belongs.</param>
        /// <param name="permittedRoleId">The role to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Delete(ulong guildId, ulong permittedRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var roleToDelete = await GetByGuild(guildId, permittedRoleId);
            if (roleToDelete is null)
            {
                return;
            }

            context.Remove(permittedRoleId);
            await context.SaveChangesAsync();
        }
    }
}