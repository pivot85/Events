namespace Events.Data.Context
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="IDesignTimeDbContextFactory{TContext}"/> for <see cref="EventDbContext"/>
    /// </summary>
    public class EventDbContextFactory : IDesignTimeDbContextFactory<EventDbContext>
    {
        /// <inheritdoc/>
        public EventDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Config.jsonc", false, true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(
                    configuration["EventsDatabase"],
                    new MySqlServerVersion(new Version(8, 0, 21)));

            return new EventDbContext(optionsBuilder.Options);
        }
    }
}