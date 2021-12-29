using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using MigraDoc.DocumentObjectModel;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    /// <summary>
    /// non generic class for hotreload in dotnet 6
    /// </summary>
    internal class WachtLijstFactuurDocumentConstructor
    {
        private const string bodyMarginLeft = "1cm";
        private const string MarginLeftRight = "1cm";
        private readonly string _imagesPath = ".\\Images";

        private readonly IThurledeStorageRepository ThurledeStorageRepository;
        private WachtlijstFactuur data;
        private Document document;

        public WachtLijstFactuurDocumentConstructor(WachtlijstFactuur data, IThurledeStorageRepository ThurledeStorageRepository)
        {
            this.document = new Document();
            document.DefaultPageSetup.LeftMargin = bodyMarginLeft;
            document.DefaultPageSetup.RightMargin = bodyMarginLeft;
            this.data = data;
            this.ThurledeStorageRepository = ThurledeStorageRepository;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
        }

        public async Task<Document> Create()
        {
            DefineStyles();
            await DefineMainContentSection();
            return document;
        }

        public async Task<string> FetchImageFromStorage(string reference)
        {
            var image = await ThurledeStorageRepository.GetFileFromStorage(reference);
            using var reader = new MemoryStream();
            image.Stream.CopyTo(reader);
            var base64 = Convert.ToBase64String(reader.ToArray());

            return $"base64:{base64}";
        }

        internal void DefineStyles()
        {
            var style = document.Styles["Normal"];

            style.Font.Name = "OpenSans";
            style.Font.Color = Colors.Black;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;

            document.Styles.AddStyle("QuestionHeader", "Normal");
            document.Styles.AddStyle("QuestionBody", "Normal");
            document.Styles.AddStyle("Header", "Normal");
            document.Styles.AddStyle("Footer", "Normal");

            style = document.Styles["Header"];
            style.Font.Size = "14pt";
            style.Font.Bold = false;
            style.Font.Italic = false;
            style.ParagraphFormat.SpaceAfter = "1cm";

            style = document.Styles["QuestionHeader"];
            style.Font.Size = 10;
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.ParagraphFormat.SpaceAfter = "1cm";

            style.ParagraphFormat.SpaceBefore = "0.1cm";
            style.ParagraphFormat.SpaceAfter = "0.2cm";

            style = document.Styles["QuestionBody"];
            style.Font.Size = 10;
            style.Font.Bold = false;
            style.Font.Italic = false;
            style.ParagraphFormat.SpaceBefore = "0cm";
            style.ParagraphFormat.SpaceAfter = "1cm";

            style = document.Styles["Footer"];
            style.ParagraphFormat.TabStops.ClearAll();
            // style.ParagraphFormat.TabStops.AddTabStop(Unit.FromMillimeter(80), TabAlignment.Center);
            style.ParagraphFormat.TabStops.AddTabStop(Unit.FromMillimeter(158), TabAlignment.Right);
        }

        private async Task DefineMainContentSection()
        {
            document.AddSection();
            await InjectHeader();
            InjectFooter();
            document.LastSection.PageSetup.LeftMargin = MarginLeftRight;
            document.LastSection.PageSetup.RightMargin = MarginLeftRight;
            document.LastSection.PageSetup.TopMargin = "0cm";
            document.LastSection.PageSetup.BottomMargin = "1cm";
            await InjectInvoiceBody();
        }

        private async Task<string> FetchOptionalImage(string reference)
        {
            string base64Image;
            try
            {
                base64Image = await FetchImageFromStorage("docatt/" + reference.ToString());
            }
            catch (FileNotFoundException)
            {
                base64Image = await FetchImageFromStorage("notfound");
            }
            if (string.IsNullOrEmpty(base64Image))
            {
                throw new Exception("Image Not set");
            }

            return base64Image;
        }

        private void InjectFooter()
        {
            var MainSection = document.Sections[0];
            var paragraph = MainSection.Footers.Primary.AddParagraph();

            paragraph.Style = "Footer";
            paragraph.AddPageField();
            paragraph.AddTab();
            paragraph.AddFormattedText("Schiedamse volkstuinvereniging Thurlede", new Font() { Color = Colors.LightGray, Size = "8Pt" });
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("https://www.svthurlede.nl", new Font() { Color = Colors.LightGray, Size = "8Pt" });
        }

        private async Task InjectHeader()
        {
            await Task.Delay(1);
        }

        private async Task InjectInvoiceBody()
        {
            var p = document.LastSection.AddParagraph();
            p.Style = "Normal";
            var i = p.AddImage(await FetchImageFromStorage("logo"));
            i.Width = "10cm";
            i.LockAspectRatio = true;
            p = document.LastSection.AddParagraph();

            p.AddText("Schiedamse Volkstuinvereniging Thurlede"); p.AddLineBreak();
            p.AddText("Parkweg 428"); p.AddLineBreak();
            p.AddText("3121 KK Schiedam"); p.AddLineBreak();
            p.AddText("www.svthurlede.nl"); p.AddLineBreak();
            p.AddText("bestuur@svthurlede.nl"); p.AddLineBreak();

            p = document.LastSection.AddParagraph();
            p.Format.SpaceBefore = "1cm";
            p.AddText($"{data.Name}"); p.AddLineBreak();
            p.AddText($"{data.Address}"); p.AddLineBreak();
            p.AddText($"{data.PostCode}"); p.AddLineBreak();
            p.AddText($"{data.City}"); p.AddLineBreak();

            p = document.LastSection.AddParagraph();
            p.Format.SpaceBefore = "1cm";
            p.AddText($"Datum :{data.InvoiceDate.ToShortDateString()}"); p.AddLineBreak();
            p.AddText($"Referentie : {data.PaymentReference}"); p.AddLineBreak();
            p.AddText($"Betreft : Wachtlijst contributie"); p.AddLineBreak();

            p = document.LastSection.AddParagraph();
            p.Format.SpaceBefore = "1cm";
            var bodyText = $@"Beste {data.Name},

Volgens onze administratie staat u bij ons ingeschreven voor een volkstuin, per jaar vragen wij een bijdrage om lid te blijven. Dit doen we om de wachtlijst accuraat te houden. Het bedrag wat u betaald heeft met de eerste inschrijving is ten gunste van de vereniging. Het bedrag wat wij nu in rekening brengen (en eventueel volgende jaren) wordt verrekend met de aanschaf van het huisje.

Ik verzoek daarom dan ook of uw {data.Amount:c2} aan de volkstuin over te maken, u kunt dit bedrag overmaken op

NL64INGB0000470288 t.n.v. Schiedamse volkstuin vereniging Thurlede
wilt u de bij de betaling dit het volgende factuurnummer vermelden

{data.PaymentReference}

Ter verificatie kunt u de bankgegevens controleren op de onze website:
Contact | Volkstuinvereniging Thurlede (svthurlede.nl)

Mochten wij voor {data.DueDate.ToShortDateString()} geen betaling hebben ontvangen van u, dan gaan wij er vanuit dat u niet (langer) geïnteresseerd bent in een volkstuin bij Thurlede en wordt u van de wachtlijst verwijderd.

Met vriendelijke groet

Marco Pietersma
Penningmeester";
            p.AddText(bodyText); p.AddLineBreak();
        }
    }
}