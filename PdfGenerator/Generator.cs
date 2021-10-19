using Macaw.Pdf.Documents.CWD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Macaw.Pdf
{
    public class Generator
    {
        private readonly ILogger<Generator> logger;
        private readonly IMigraDocService<SomeReport> migraDocService;

        public Generator(ILogger<Generator> logger, IMigraDocService<SomeReport> migraDocService)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
        }

        [FunctionName("Create")]
        public IActionResult CreateCWDQuestionaire(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/Questionaire")] HttpRequest req)
        {
            var data = new SomeReport();

            var path = migraDocService.CreateMigraDocPdf(data);

            return new FileContentResult(File.ReadAllBytes(path), "application/pdf")
            {
                FileDownloadName = "Export.pdf"
            };
        }
    }
}