namespace Events.Data.DataAccessLayer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Events.Data.Models;

    /// <summary>
    /// Implementation Contract of <see cref="PermittedRoleDataAccessLayer"/>
    /// </summary>
    internal interface IPermittedRole
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