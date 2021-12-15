using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class CWDGenerator
    {
        private const string FunctionNamePrefix = "CWD";
        private readonly ILogger<CWDGenerator> logger;
        private readonly IMigraDocService<CWDDocumentData> migraDocService;
        private readonly ISendGridService sendGridService;
        private readonly ICWDStorageRepository storageRepository;

        public CWDGenerator(ILogger<CWDGenerator> logger, IMigraDocService<CWDDocumentData> migraDocService, ICWDStorageRepository storageRepository, ISendGridService sendGridService)
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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/Questionaire")] HttpRequest req, ExecutionContext context)
        {
            CWDDocumentData CWDDocument = null;
            try
            {
                var data = new CWDReport();

                var serializer = new JsonSerializer();
                using var sr = new StreamReader(req.Body);
                using var jsonTextReader = new JsonTextReader(sr);
                CWDDocument = serializer.Deserialize<CWDDocumentData>(jsonTextReader);

                CWDDocument.NOKAntwoorden = JsonConvert.DeserializeObject<IEnumerable<NOKAntwoord>>(CWDDocument.NOKAntwoordenString);
                CWDDocument.OverigeAntwoorden = JsonConvert.DeserializeObject<IEnumerable<OverigAntwoord>>(CWDDocument.OtherAntwoordenString);
            }
            catch (System.Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            migraDocService.FontDirectory = Path.Combine(context.FunctionAppDirectory, "Resources");
            var path = await migraDocService.CreateMigraDocPdf(CWDDocument);

            var properties = new Dictionary<string, string>
            {
                { nameof(CWDDocument.Inspecteur), CWDDocument.Inspecteur }
            };

            await sendGridService.SendPdfToRecipient(new SendGrid.Helpers.Mail.EmailAddress(CWDDocument.EmailRapport, CWDDocument.Manager),
                "Nieuw Inspectie Rapport",
                 $"Hierbij een nieuw inspectie rapport van {{{{{nameof(CWDDocument.Inspecteur)}}}}}",
                 properties,
                 path);

            return new OkResult();
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

        [FunctionName(FunctionNamePrefix + nameof(StoreImageFromBase64))]
        public async Task<IActionResult> StoreImageFromBase64([HttpTrigger(AuthorizationLevel.Function, "post", Route = "CWD/QuestionaireImageBase64")] HttpRequest req)
        {
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(req.Body);
            using var jsonTextReader = new JsonTextReader(sr);

            var image = JToken.ReadFrom(jsonTextReader);
            var fotoId = image["fotoId"].Value<string>();
            var bytes = Convert.FromBase64String(image["foto"].Value<string>());
            var contents = new StreamContent(new MemoryStream(bytes));

            await storageRepository.WriteFileToStorage(fotoId, ".png", contents.ReadAsStream());

            return new OkResult();
        }
    }
}