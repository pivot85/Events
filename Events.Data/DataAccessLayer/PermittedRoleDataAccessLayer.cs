using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events.Data.Context;
using Events.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Data.DataAccessLayer
{
    public class PermittedRoleDataAccessLayer : IPermittedRole
    {
        private readonly EventDbContext _dbContext;

        public PermittedRoleDataAccessLayer(EventDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PermittedRole> GetPermittedRole(ulong guildId, ulong permittedRole)
        {
            return await _dbContext.PermittedRoles
                .Where(x => x.GuildId == guildId && x.Id == permittedRole)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PermittedRole>> GetPermittedRoles(ulong guildId)
        {
            return await _dbContext.PermittedRoles
                .ToListAsync();
        }

        public async Task CreatePermittedRole(ulong guildId, ulong permittedRoleId)
        {
            _dbContext.Add(new PermittedRole
            {
                Id = permittedRoleId,
                GuildId = guildId
            });

            await _dbContext.SaveChangesAsync();
        }

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