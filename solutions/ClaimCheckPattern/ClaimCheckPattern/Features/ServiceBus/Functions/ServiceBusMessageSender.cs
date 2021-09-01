using Azure.Storage.Blobs;
using ClaimCheckPattern.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ClaimCheckPattern.Features.ServiceBus.Functions
{
    public class ServiceBusMessageSender
    {
        private readonly IAzureClientFactory<BlobServiceClient> _service;

        public ServiceBusMessageSender(IAzureClientFactory<BlobServiceClient> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [FunctionName(nameof(ServiceBusMessageSender))]
        [return: ServiceBus("%TestTopic%", EntityType = EntityType.Topic, Connection = "SbConnectionString")]
        public async Task<Message> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var message = new Message
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            using (MemoryStream memoryStream = new MemoryStream())
            { 
                await req.Body.CopyToAsync(memoryStream);

                message.Body = memoryStream.ToArray();
            }

            await message.ClaimCheckOnSend(_service.CreateClient("ServiceBusMessageStorage"));

            return message;
        }
    }
}
