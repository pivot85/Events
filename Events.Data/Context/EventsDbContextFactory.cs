namespace Events.Data.Context
{
    using System;
    using System.IO;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The <see cref="IDesignTimeDbContextFactory{TContext}"/> for <see cref="EventsDbContext"/>
    /// </summary>
    public class EventsDbContextFactory : IDesignTimeDbContextFactory<EventsDbContext>
    {
        /// <inheritdoc/>
        public EventsDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseMySql(
                    configuration.GetConnectionString("Default"),
                    new MySqlServerVersion(new Version(8, 0, 21)));

            return new EventsDbContext(optionsBuilder.Options);
        }
    }
}