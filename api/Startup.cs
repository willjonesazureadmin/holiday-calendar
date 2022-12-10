using System;
using System.IO;
using Azure.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;

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
            
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(config.GetValue<string>("Values:PrivateStorageAccountName"), config.GetValue<string>("Values:PrivateStorageKey"));
            builder.Services.AddAzureClients(builder => {
                builder.AddBlobServiceClient(new Uri(String.Format(config.GetValue<string>("Values:PrivateStorageServiceUri"), config.GetValue<string>("Values:PrivateStorageAccountName"))),credential);
                builder.UseCredential(new DefaultAzureCredential(includeInteractiveCredentials: true));
            });
        }

    }
}
