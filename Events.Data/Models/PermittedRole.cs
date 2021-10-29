namespace Events.Data.Models
{
    /// <summary>
    /// Defines the roles that are able to create events on a server.
    /// </summary>
    public class PermittedRole
    {
        /// <summary>
        /// Gets or Sets the Id of the role.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the guild that the role belongs.
        /// </summary>
        public ulong GuildId { get; set; }
    }
}