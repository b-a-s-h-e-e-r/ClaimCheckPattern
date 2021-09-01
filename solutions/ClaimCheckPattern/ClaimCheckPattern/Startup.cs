using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(ClaimCheckPattern.Startup))]
namespace ClaimCheckPattern
{
    [ExcludeFromCodeCoverage]
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();

            builder.Services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(context.Configuration["StorageConnectionString"]).WithName("ServiceBusMessageStorage");
            });
        }
    }
}
