using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using System;
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

                default:
                    throw new InvalidOperationException($"Documenttype {thurledeDoc.DocumentType} is not supported");
            }
        }

        public async Task<Document> CreateWerkbeurtFactuur(T pdfData)
        {
            var data = pdfData as ThurledeFactuur;
            data.Subject = "Gemiste Werkbeurt"
            var constructor = new ThurledeFactuurDocumentConstructor(data, ThurledeStorageRepository);

            return await constructor.Create((document, dataObj) =>
            {
                var plural = "en";
                var data = pdfData as WerkbeurtFactuur;

                var bodyText = $@"
Beste {data.Name}

Volgens onze administratie heeft u in 2021 {data.AantalWerkbeurtenGemist} werkbeurt(en) gemist, In het huishoudelijk regelement staat beschreven dat er bij het missen van een werkbeurt zonder afmelding / motivitie een vergoeding inrekening gebracht wordt.
Deze vergoeding is {data.PrijsPerWerkbeurt:C2} per gemiste werkbeurt.

{data.InvoiceBodyText}

Daardoor doen we u een rekening toekomen voor de gemiste werktbeurten.
Graag ontvangen wij van u op onze rekening 
NL64INGB0000470288 t.n.v. Schiedamse volkstuin vereniging Thurlede, een bedrag van { data.AantalWerkbeurtenGemist * data.PrijsPerWerkbeurt:C2}

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