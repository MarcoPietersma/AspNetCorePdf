using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class DemoGenerator
    {
        private const string FunctionNamePrefix = "Demo";

        private readonly ILogger<CWDGenerator> logger;
        private readonly IMigraDocService<DemoDocumentData> migraDocService;
        private readonly ICWDStorageRepository storageRepository;

        public DemoGenerator(ILogger<CWDGenerator> logger, IMigraDocService<DemoDocumentData> migraDocService, ICWDStorageRepository storageRepository)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
            this.storageRepository = storageRepository;
        }

        [FunctionName(FunctionNamePrefix + nameof(Create))]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Demo/DemoDocument")] HttpRequest req)
        {
            using var sr = new StreamReader(req.Body);
            using var jsonTextReader = new JsonTextReader(sr);

            var t = await sr.ReadToEndAsync();

            var content = JsonConvert.DeserializeObject<DemoDocumentData>(t);

            if (string.IsNullOrEmpty(content.DocumentName))
            {
                return new BadRequestResult();
            }

            var path = await migraDocService.CreateMigraDocPdf(content);

            return new FileContentResult(await File.ReadAllBytesAsync(path), "application/pdf")
            {
                FileDownloadName = "Export.pdf"
            };
        }

        [FunctionName(FunctionNamePrefix + nameof(FetchImage))]
        public async Task<ActionResult> FetchImage([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Demo/DemoImage")] HttpRequest req)
        {
            var reference = req.Query["Reference"];

            try
            {
                var responseStream = await storageRepository.GetFileFromStorage(reference);
                return new FileStreamResult(responseStream.Stream, responseStream.MimeType);
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }
        }

        [FunctionName(FunctionNamePrefix + nameof(StoreImage))]
        public async Task<IActionResult> StoreImage([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Demo/DemoImage")] HttpRequest req)
        {
            var formdata = await req.ReadFormAsync();
            var file = req.Form.Files["file"];

            if (file.ContentType.Split("/")[0] != "image")
            {
                return new BadRequestResult();
            }

            await storageRepository.WriteFileToStorage(formdata["requestIdentifier"].ToString(), file.FileName.Split(".")[1], file.OpenReadStream());

            return new OkResult();
        }
    }
}