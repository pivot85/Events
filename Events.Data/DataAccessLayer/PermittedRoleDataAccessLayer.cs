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
        private readonly EventDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermittedRoleDataAccessLayer"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="EventDbContext"/> to be injected.</param>
        public PermittedRoleDataAccessLayer(EventDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get a single permitted role.
        /// </summary>
        /// <param name="guildId">The guild which the role belongs.</param>
        /// <param name="permittedRole">The role being searched for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task<PermittedRole> GetPermittedRole(ulong guildId, ulong permittedRole)
        {
            return await _dbContext.PermittedRoles
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
            return await _dbContext.PermittedRoles
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
            _dbContext.Add(new PermittedRole
            {
                Id = permittedRoleId,
                GuildId = guildId,
            });

            await _dbContext.SaveChangesAsync();
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
            var roleToUpdate = await GetPermittedRole(guildId, permittedRoleId);
            if (roleToUpdate is null)
            {
                return;
            }

            roleToUpdate.Id = newPermittedRoleId;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a Permitted Role.
        /// </summary>
        /// <param name="guildId">The guild in which the role belongs.</param>
        /// <param name="permittedRoleId">The role to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task DeletePermittedRole(ulong guildId, ulong permittedRoleId)
        {
            var roleToDelete = await GetPermittedRole(guildId, permittedRoleId);
            if (roleToDelete is null)
            {
                return;
            }

            _dbContext.Remove(permittedRoleId);
            await _dbContext.SaveChangesAsync();
        }
    }
}