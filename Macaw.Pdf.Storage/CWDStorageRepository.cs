namespace Macaw.Pdf.Storage
{
    using Azure.Storage.Blobs;
    using Macaw.Pdf.Interfaces;
    using Microsoft.Extensions.Configuration;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class CWDStorageRepository : ICWDStorageRepository
    {
        private const string ContainerName = "cwd";
        private readonly IConfiguration _configuration;

        public CWDStorageRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Stream> GetFileFromStorage(string reference, string extension)
        {
            var container = await CreateClient();
            var fileName = $"{reference}.{extension}";

            var blobClient = container.GetBlobClient(fileName);

            var d = await blobClient.DownloadStreamingAsync();
            return d.Value.Content;
        }

        public async Task<string> ReadTextFromFile(string filename)
        {
            var container = await CreateClient();
            var blob = container.GetBlobClient("MailTemplates/" + filename);

            var content = await blob.DownloadContentAsync();

            var stringContent = Encoding.UTF8.GetString(content.Value.Content);

            return stringContent;
        }

        public async Task WriteFileToStorage(string reference, string extension, Stream fileSteam)
        {
            var container = await CreateClient();

            var fileName = $"{reference}.{extension}";

            await container.UploadBlobAsync(fileName, fileSteam);
        }

        private async Task<BlobContainerClient> CreateClient()
        {
            var blobServiceClient = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
            var container = blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}