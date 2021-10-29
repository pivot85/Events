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
    public class PermittedRoleDataAccessLayer : IPermittedRole
    {
        private readonly IDbContextFactory<EventsDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermittedRoleDataAccessLayer"/> class.
        /// </summary>
        /// <param name="contextFactory">The <see cref="IDbContextFactory{TContext}"/> to be injected.</param>
        public PermittedRoleDataAccessLayer(IDbContextFactory<EventsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get a single permitted role.
        /// </summary>
        /// <param name="guildId">The guild which the role belongs.</param>
        /// <param name="permittedRole">The role being searched for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<PermittedRole> GetPermittedRole(ulong guildId, ulong permittedRole)
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
        public async Task<IEnumerable<PermittedRole>> GetPermittedRoles(ulong guildId)
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
        public async Task CreatePermittedRole(ulong guildId, ulong permittedRoleId)
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
        /// Update a permitted role with a new one.
        /// </summary>
        /// <param name="guildId">The guild in which the role belongs.</param>
        /// <param name="permittedRoleId">The role to be updated.</param>
        /// <param name="newPermittedRoleId">The Id of the new role.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdatePermittedRole(ulong guildId, ulong permittedRoleId, ulong newPermittedRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var roleToUpdate = await GetPermittedRole(guildId, permittedRoleId);
            if (roleToUpdate is null)
            {
                return;
            }

            roleToUpdate.Id = newPermittedRoleId;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a Permitted Role.
        /// </summary>
        /// <param name="guildId">The guild in which the role belongs.</param>
        /// <param name="permittedRoleId">The role to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeletePermittedRole(ulong guildId, ulong permittedRoleId)
        {
            using var context = _contextFactory.CreateDbContext();

            var roleToDelete = await GetPermittedRole(guildId, permittedRoleId);
            if (roleToDelete is null)
            {
                return;
            }

            context.Remove(permittedRoleId);
            await context.SaveChangesAsync();
        }
    }
}