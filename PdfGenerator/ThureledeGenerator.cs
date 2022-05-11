using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Services;
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
    public class ThurledeGenerator
    {
        private const string FunctionNamePrefix = "Thurlede";
        private readonly ILogger<ThurledeGenerator> logger;
        private readonly IMigraDocService<ThurledeFactuur> migraDocService;
        private readonly ISendGridService sendGridService;
        private readonly IThurledeStorageRepository storageRepository;

        public ThurledeGenerator(ILogger<ThurledeGenerator> logger, IMigraDocService<ThurledeFactuur> migraDocService, IThurledeStorageRepository storageRepository, ISendGridService sendGridService)
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

        [FunctionName(FunctionNamePrefix + nameof(ThurledeFactuur))]
        public async Task<IActionResult> ThurledeFactuur(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Thurlede/{FactuurType}")] HttpRequest req, string FactuurType, ExecutionContext context)
        {
            ThurledeFactuur thurledeFactuur;

            ThurledeFactuur ThurledeDocument = null;
            try
            {
                var serializer = new JsonSerializer();
                using var sr = new StreamReader(req.Body);
                var BodyText = await sr.ReadToEndAsync();

                switch (FactuurType)
                {
                    case nameof(WachtlijstFactuur):
                        ThurledeDocument = JsonConvert.DeserializeObject<WachtlijstFactuur>(BodyText);
                        break;

                    case "WerkbeurtFactuur":
                        ThurledeDocument = JsonConvert.DeserializeObject<WerkbeurtFactuur>(BodyText);
                        break;

                    case "ContributieFactuur":
                        ThurledeDocument = JsonConvert.DeserializeObject<ContributieFactuur>(BodyText);
                        break;

                    default:
                        return new BadRequestResult();
                }
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            ThurledeDocument.DocumentType = FactuurType;

            migraDocService.FontDirectory = Path.Combine(context.FunctionAppDirectory, "Resources");
            var path = await migraDocService.CreateMigraDocPdf(ThurledeDocument);

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