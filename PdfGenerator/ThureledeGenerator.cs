using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class ThurledeGenerator
    {
        private const string FunctionNamePrefix = "Thurlede";
        private readonly ILogger<ThurledeGenerator> logger;
        private readonly IMigraDocService<WachtlijstFactuur> migraDocService;
        private readonly ISendGridService sendGridService;
        private readonly IThurledeStorageRepository storageRepository;

        public ThurledeGenerator(ILogger<ThurledeGenerator> logger, IMigraDocService<WachtlijstFactuur> migraDocService, IThurledeStorageRepository storageRepository, ISendGridService sendGridService)
        {
            this.logger = logger;
            this.migraDocService = migraDocService;
            this.storageRepository = storageRepository;
            this.sendGridService = sendGridService;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [FunctionName(FunctionNamePrefix + nameof(Create))]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Thurlede/WachtlijstFactuur")] HttpRequest req, ExecutionContext context)
        {
            WachtlijstFactuur ThurledeDocument = null;
            try
            {
                var serializer = new JsonSerializer();
                using var sr = new StreamReader(req.Body);
                var BodyText = await sr.ReadToEndAsync();

                ThurledeDocument = JsonConvert.DeserializeObject<WachtlijstFactuur>(BodyText);
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            migraDocService.FontDirectory = Path.Combine(context.FunctionAppDirectory, "Resources");
            var path = await migraDocService.CreateMigraDocPdf(ThurledeDocument);

#if !DEBUG

            await sendGridService.SendPdfToRecipient(new SendGrid.Helpers.Mail.EmailAddress(ThurledeDocument.EmailRapport, ThurledeDocument.Manager),
                "Nieuw Inspectie Rapport",
                 $"Hierbij een nieuw inspectie rapport van {{{nameof(ThurledeDocument.Inspecteur)}}}",
                 properties,
                 path);
#else
            var filename = Path.Combine("d:\\temp\\Thurlede\\", DateTime.Now.ToString("yyyyMMddHHmm")
            + ".pdf"); File.Copy(path, filename, true);
            migraDocService.Clean(ThurledeDocument);
#endif

            try
            {
                var responseStream = File.OpenRead(path);
                return new FileStreamResult(responseStream, "application/pdf");
            }
            catch (FileNotFoundException)
            {
                return new BadRequestResult();
            }
        }
    }
}