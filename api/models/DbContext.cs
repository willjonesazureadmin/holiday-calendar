using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace api.models
{
    public class ReadWriteContext : DbContext
    {
        public ReadWriteContext(DbContextOptions options) : base(options)
        { }
        public DbSet<CalendarEntry> CalendarEntry { get; set; }
        public DbSet<Calendar> Calendar {get; set;}


    }

    public class MyDbContextFactory : IDesignTimeDbContextFactory<ReadWriteContext>
    {      

        ReadWriteContext IDesignTimeDbContextFactory<ReadWriteContext>.CreateDbContext(string[] args)
        {
                    var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            var builder = new DbContextOptionsBuilder<ReadWriteContext>();
            var connectionString = config.GetValue<string>("Values:SqldbConnectionString");

            builder.UseSqlServer(connectionString);

            return new ReadWriteContext(builder.Options);
        }
    }




}
