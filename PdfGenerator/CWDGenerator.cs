using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class CWDGenerator
    {
        private readonly ILogger<CWDGenerator> logger;
        private readonly IMigraDocService<CWDReport> migraDocService;
        private readonly ICWDStorageRepository storageRepository;

        public CWDGenerator(ILogger<CWDGenerator> logger, IMigraDocService<CWDReport> migraDocService, ICWDStorageRepository storageRepository)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
            this.storageRepository = storageRepository;
        }

        [FunctionName("Create")]
        public IActionResult CreateCWDQuestionaire(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/Questionaire")] HttpRequest req)
        {
            var data = new CWDReport();

            var path = migraDocService.CreateMigraDocPdf(data);

            return new FileContentResult(File.ReadAllBytes(path), "application/pdf")
            {
                FileDownloadName = "Export.pdf"
            };
        }

        [FunctionName(nameof(FetchImage))]
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

        [FunctionName(nameof(StoreImage))]
        public async Task<IActionResult> StoreImage([HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/QuestionaireImage")] HttpRequest req)
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