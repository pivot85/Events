using System.Collections.Generic;
using System.Threading.Tasks;
using Events.Data.Models;

namespace Events.Data.DataAccessLayer
{
    public interface IPermittedRole
    {
        // Get
        public Task<PermittedRole> GetPermittedRole(ulong guildId, ulong permittedRole);
        public Task<IEnumerable<PermittedRole>> GetPermittedRoles(ulong guildId);

        // Create
        public Task CreatePermittedRole(ulong guildId, ulong permittedRoleId);
        
        // Update
        public Task UpdatePermittedRole(ulong guildId, ulong permittedRoleId, ulong newRoleId);
        
        // Delete
        public Task DeletePermittedRole(ulong guildId, ulong permittedRoleId);
    }
}