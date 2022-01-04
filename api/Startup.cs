using System;
using System.IO;
using api.models;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(api.Startup))]


namespace api
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
            
            builder.Services.AddDbContext<ReadWriteContext>(options => options.UseSqlServer(config.GetValue<string>("Values:SqldbConnectionString")));   
            builder.Services.AddAzureClients(builder => {
                builder.AddBlobServiceClient(config.GetSection("Values:PrivateStorage"));
                builder.UseCredential(new DefaultAzureCredential());
            });
        }

    }
}
