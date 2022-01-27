using System;
using System.IO;
using api.models;
using Azure.Identity;
using Azure.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
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
            
            builder.Services.AddDbContext<ReadWriteContext>(options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, config.GetValue<string>("Values:SqldbConnectionString")));   
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(config.GetValue<string>("Values:PrivateStorage:AccountName"), config.GetValue<string>("Values:PrivateStorage:Key"));
            builder.Services.AddAzureClients(builder => {
                builder.AddBlobServiceClient(new Uri(config.GetValue<string>("Values:PrivateStorage:ServiceUri")),credential);
                builder.UseCredential(new DefaultAzureCredential(includeInteractiveCredentials: true));
            });
        }

    }
}
