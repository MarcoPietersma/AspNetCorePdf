using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class ThurledeMigraDocService<T> : MigraDocService<T> where T : IPdfData
    {
        private readonly IThurledeStorageRepository ThurledeStorageRepository;

        public ThurledeMigraDocService(IThurledeStorageRepository ThurledeStorageRepository) : base()
        {
            this.ThurledeStorageRepository = ThurledeStorageRepository;
        }

        public async Task<Document> CreateContributieFactuur(T pdfData)
        {
            var data = pdfData as ThurledeFactuur;

            var constructor = new ThurledeFactuurDocumentConstructor(data, ThurledeStorageRepository);

            return await constructor.Create((document, dataObj) =>
            {
                var data = pdfData as ContributieFactuur;

                var p = document.LastSection.AddParagraph();
                p.AddText(@"Hierbij doen wij u de rekening toekomen betreffende de ledenbijdrage van uw tuin over het jaar 2022 op ons complex ‘Schiedamse Volkstuinvereniging Thurlede’ zoals onderstaand gespecificeerd.");
                p.AddLineBreak();
                p.AddLineBreak();
                var table = document.LastSection.AddTable();

                table.Style = "Table";

                table.Borders.Color = Colors.Gray;
                table.Borders.Width = 0.25;
                table.Borders.Left.Width = 0.5;
                table.Borders.Right.Width = 0.5;
                table.Rows.LeftIndent = 0;

                var column = table.AddColumn("8cm");
                column.Format.Alignment = ParagraphAlignment.Left;

                column = table.AddColumn("3cm");
                column.Format.Alignment = ParagraphAlignment.Right;

                column = table.AddColumn("3.5cm");
                column.Format.Alignment = ParagraphAlignment.Right;
                column = table.AddColumn("3.5cm");
                column.Format.Alignment = ParagraphAlignment.Right;
                var row = table.AddRow();
                row.HeadingFormat = true;
                row.Format.Alignment = ParagraphAlignment.Center;
                row.Format.Font.Bold = true;

                row.Cells[0].AddParagraph("Omschrijving");
                row.Cells[1].AddParagraph("Aantal");
                row.Cells[2].AddParagraph("Prijs p.s.");
                row.Cells[3].AddParagraph("Totaal");

                foreach (var line in data.InvoiceLines)
                {
                    row = table.AddRow();

                    row.Cells[0].AddParagraph(line.Description);
                    row.Cells[1].AddParagraph(line.NumberOfItems.ToString());
                    row.Cells[2].AddParagraph(line.PricePerItem.ToString("C2"));
                    row.Cells[3].AddParagraph(line.Total.ToString("C2"));
                }

                row = table.AddRow();

                var rp = row.Cells[0].AddParagraph();
                rp.AddFormattedText("Totaal", new Font() { Bold = true });
                row.Cells[3].AddParagraph(data.InvoiceLines.Sum(e => e.Total).ToString("C2"));

                p = document.LastSection.AddParagraph();
                p.AddLineBreak();

                p.AddText(
$@"Het bedrag dient u voor {data.DueDate.ToShortDateString()} over te maken o.v.v. {data.PaymentReference} op onze rekening NL64INGB0000470288 t.n.v. Schiedamse Volkstuinvereniging Thurlede.

U mag zelf bepalen of u het bedrag in delen of in 1 keer betaald, wel verwachten we het totale bedrag voor 1 juni op onze rekening.
Door de uiterste betaaldatum naar 1 juni te zetten zien wij af van andere betaalregelingen.

Met vriendelijke groet

Marco Pietersma
Penningmeester");
            });
        }

        public override async Task<Document> CreateDocument(T pdfData)
        {
            var thurledeDoc = pdfData as ThurledeFactuur;
            if (thurledeDoc == null)
                return null;

            switch (thurledeDoc.DocumentType)
            {
                case nameof(WachtlijstFactuur):
                    return await CreateWachtlijstFactuur(pdfData);

                case nameof(WerkbeurtFactuur):
                    return await CreateWerkbeurtFactuur(pdfData);

                case nameof(ContributieFactuur):
                    return await CreateContributieFactuur(pdfData);

                default:
                    throw new InvalidOperationException($"Documenttype {thurledeDoc.DocumentType} is not supported");
            }
        }

        public async Task<Document> CreateWerkbeurtFactuur(T pdfData)
        {
            var data = pdfData as ThurledeFactuur;
            data.Subject = "Gemiste Werkbeurt";
            var constructor = new ThurledeFactuurDocumentConstructor(data, ThurledeStorageRepository);

            return await constructor.Create((document, dataObj) =>
            {
                var data = pdfData as WerkbeurtFactuur;

                var bodyText = $@"
Beste {data.Name}

Volgens onze administratie heeft u in 2021 {data.AantalWerkbeurtenGemist} werkbeurt(en) gemist, In het huishoudelijk regelement staat beschreven dat er bij het missen van een werkbeurt zonder afmelding / motivitie een vergoeding inrekening gebracht wordt.
Deze vergoeding is {data.PrijsPerWerkbeurt:C2} per gemiste werkbeurt.

{data.InvoiceBodyText}

Daardoor doen we u een rekening toekomen voor de gemiste werktbeurten.
Graag ontvangen wij van u op onze rekening
NL64INGB0000470288 t.n.v. Schiedamse volkstuin vereniging Thurlede, een bedrag van {data.AantalWerkbeurtenGemist * data.PrijsPerWerkbeurt:C2}

Mocht u het niet eens zijn met deze rekening of mocht de registratie van uw werkbeurten niet kloppen horen wij dat graag van u. U kunt reageren door antwoorden op deze mail.
Uiteraard zien wij u liever op de werkbeurt als dat wij een rekening na moeten sturen, wij hopen u in het volgende seizoen weer bij de werkbeurten te kunnen verwelkomen.

Met vriendelijke groet

Marco Pietersma
Penningmeester
";

                var p = document.LastSection.AddParagraph();
                p.Format.SpaceBefore = "1cm";
                p.AddText(bodyText);
            });
        }

        private async Task<Document> CreateWachtlijstFactuur(T pdfData)
        {
            var data = pdfData as WachtlijstFactuur;
            data.Subject = "Contributie Thurlede";
            var constructor = new ThurledeFactuurDocumentConstructor(data, ThurledeStorageRepository);

            return await constructor.Create((document, dataObj) =>
            {
                var data = dataObj as WachtlijstFactuur;
                var p = document.LastSection.AddParagraph();
                p.Format.SpaceBefore = "1cm";

                var bodyText = $@"
Beste {data.Name},

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
            });
        }
    }
}