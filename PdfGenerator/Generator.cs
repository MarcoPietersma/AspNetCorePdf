using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Macaw.Pdf.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Macaw.Pdf
{
    public class Generator
    {
        private readonly ILogger<Generator> logger;
        private readonly IMigraDocService migraDocService;

        public Generator(ILogger<Generator> logger, IMigraDocService migraDocService)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
        }

        [FunctionName("Create")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            PdfData data = new PdfData {
                DocumentTitle = "Title of the MigraDoc",
                DocumentName = "MigraDocDocName",
                CreatedBy = "Damien",
                Description = "some data description which I have, and want to display in the PDF file..., This is another text, what is happening here, why is this text display...",
                DisplayListItems = new List<ItemsToDisplay>
                 {
                    new ItemsToDisplay{ Id = "Print Servers", Data1= "some data", Data2 = "more data to display"},
                    new ItemsToDisplay{ Id = "Network Stuff", Data1= "IP4", Data2 = "any left"},
                    new ItemsToDisplay{ Id = "Job details", Data1= "too many", Data2 = "say no"},
                    new ItemsToDisplay{ Id = "Firewall", Data1= "what", Data2 = "Let's burn it"}
                }
            };
            string path = migraDocService.CreateMigraDocPdf(data);
            return new FileContentResult(File.ReadAllBytes(path), "application/octet-stream") {
                FileDownloadName = "Export.csv"
            };
        }
    }
}