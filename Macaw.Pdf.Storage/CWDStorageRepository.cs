namespace Macaw.Pdf.Storage
{
    using Azure.Storage.Blobs;
    using HeyRed.Mime;
    using Macaw.Pdf.Interfaces;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        public async Task<(Stream Stream, string MimeType)> GetFileFromStorage(string reference)
        {
            var container = await CreateClient();

            var fileName = await GetFileByReference(reference);

            var blobClient = container.GetBlobClient(fileName);

            var d = await blobClient.DownloadStreamingAsync();
            return (Stream: d.Value.Content, MimeType: MimeTypesMap.GetMimeType(blobClient.Name));
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
            var blob = container.GetBlobClient(fileName);

            await blob.UploadAsync(fileSteam, overwrite: true);
        }

        private async Task<BlobContainerClient> CreateClient()
        {
            var blobServiceClient = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
            var container = blobServiceClient.GetBlobContainerClient(ContainerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }

        private async Task<string> GetFileByReference(string reference)
        {
            var files = await GetFiles();
            var file = files.SingleOrDefault(e => e.StartsWith($"{reference}.", System.StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrEmpty(file))
            {
                throw new FileNotFoundException("Blob not found", reference);
            }

            return file;
        }

        private async Task<IEnumerable<string>> GetFiles()
        {
            var container = await CreateClient();
            return container.GetBlobs().Select(e => e.Name).ToList();
        }
    }
}