namespace Events.Data.DataAccessLayer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Events.Data.Models;

    /// <summary>
    /// Implementation Contract of <see cref="PermittedRolesDataAccessLayer"/>
    /// </summary>
    internal interface IPermittedRolesDataAccessLayer
    {
        // Get
        public Task<PermittedRole> GetByGuild(ulong guildId, ulong permittedRole);

        public Task<IEnumerable<PermittedRole>> GetAllByGuild(ulong guildId);

        // Create
        public Task Create(ulong guildId, ulong permittedRoleId);

        // Delete
        public Task Delete(ulong guildId, ulong permittedRoleId);
    }
}