using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.ServiceBus;

namespace ClaimCheckPattern.Common.Extensions
{
    public static class MessageExtensions
    {
        private static readonly string MessageContainer = "servicebus-messages";

        public static async Task<BlobClient> GetBlobClient(BlobServiceClient blobServiceClient, string blobName)
        {
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(MessageContainer);

            await blobContainerClient.CreateIfNotExistsAsync();

            return blobContainerClient.GetBlobClient(blobName);
        }

        public static async Task ClaimCheckOnSend(this Message message, BlobServiceClient blobServiceClient)
        {
            if (message.Size > 250 * 1024)
            {
                var blobClient = await GetBlobClient(blobServiceClient, $"{message.MessageId ?? Guid.NewGuid().ToString()}.json");

                await blobClient.UploadAsync(new MemoryStream(message.Body), overwrite: true);

                message.UserProperties.Add("BlobRef", blobClient.Name);
                message.Body = Encoding.UTF8.GetBytes(blobClient.Name);
            }
        }

        public static async Task ClaimCheckOnReceive(this Message message, BlobServiceClient blobServiceClient)
        {
            if (!message.UserProperties.TryGetValue("BlobRef", out var blobRef))
            {
                return;
            }

            var blobClient = await GetBlobClient(blobServiceClient, (string)blobRef);
            var content = await blobClient.DownloadContentAsync();

            if (content.Value.Content != null) message.Body = content.Value.Content.ToArray();
            await blobClient.DeleteAsync();
        }
    }
}
