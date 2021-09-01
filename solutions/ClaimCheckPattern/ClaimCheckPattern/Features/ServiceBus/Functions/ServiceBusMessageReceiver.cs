using Azure.Storage.Blobs;
using ClaimCheckPattern.Common.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ClaimCheckPattern.Features.ServiceBus.Functions
{
    class ServiceBusMessageReceiver
    {
        private readonly IAzureClientFactory<BlobServiceClient> _blobService;

        public ServiceBusMessageReceiver(IAzureClientFactory<BlobServiceClient> blobService)
        {
            _blobService = blobService;
        }

        public IMessageReceiver MessageReceiver { get; set; }

        [FunctionName(nameof(ServiceBusMessageReceiver))]
        public async Task Run(
            [ServiceBusTrigger("%TestTopic%", "%TestTopicSubscription%", Connection = "SbConnectionString")] Message message,
            MessageReceiver messageReceiver,
            string lockToken,
            ILogger log)
        {
            if (MessageReceiver == null)
            {
                MessageReceiver = messageReceiver;
            }

            try
            {
                await message.ClaimCheckOnReceive(_blobService.CreateClient("ServiceBusMessageStorage"));
            }
            catch
            {
                await MessageReceiver.DeadLetterAsync(lockToken);
            }
        }
    }
}
