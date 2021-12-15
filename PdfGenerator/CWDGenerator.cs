using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Interfaces;
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
    public class CWDGenerator
    {
        private const string FunctionNamePrefix = "CWD";
        private readonly ILogger<CWDGenerator> logger;
        private readonly IMigraDocService<CWDDocumentData> migraDocService;
        private readonly ICWDStorageRepository storageRepository;

        public CWDGenerator(ILogger<CWDGenerator> logger, IMigraDocService<CWDDocumentData> migraDocService, ICWDStorageRepository storageRepository)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
            this.storageRepository = storageRepository;
        }

        [FunctionName(FunctionNamePrefix + nameof(Create))]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/Questionaire")] HttpRequest req)
        {
            var data = new CWDReport();

            var serializer = new JsonSerializer();

            using var sr = new StreamReader(req.Body);
            using var jsonTextReader = new JsonTextReader(sr);
            var content = serializer.Deserialize<CWDDocumentData>(jsonTextReader);

            var path = await migraDocService.CreateMigraDocPdf(content);

            return new FileContentResult(File.ReadAllBytes(path), "application/pdf")
            {
                FileDownloadName = "Export.pdf"
            };
        }

        [FunctionName(FunctionNamePrefix + nameof(FetchImage))]
        public async Task<ActionResult> FetchImage([HttpTrigger(AuthorizationLevel.Function, "get", Route = "CWD/QuestionaireImage")] HttpRequest req)
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
        public async Task<IActionResult> StoreImage([HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/QuestionaireImage")] HttpRequest req)
        {
            var formdata = await req.ReadFormAsync();

            if (formdata.Files.Count == 0)
            {
                return new BadRequestResult();
            }
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