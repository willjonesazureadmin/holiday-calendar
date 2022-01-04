using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace api.models
{
    public class ReadWriteContext : DbContext
    {
        public ReadWriteContext(DbContextOptions<ReadWriteContext> options) : base(options) {}
        public DbSet<CalendarEntry> CalendarEntry {get; set;}

    }

    public class ReadWriteContextFactory : IDesignTimeDbContextFactory<ReadWriteContext>
{
    public ReadWriteContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadWriteContext>();
        optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("SqldbConnectionString"));

        return new ReadWriteContext(optionsBuilder.Options);
    }
}

}
