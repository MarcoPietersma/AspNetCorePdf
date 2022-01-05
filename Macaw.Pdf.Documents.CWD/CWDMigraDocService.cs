using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Macaw.Pdf
{
    public class CWDMigraDocService<T> : MigraDocService<T> where T : IPdfData
    {
        private readonly ICWDStorageRepository cWDStorageRepository;

        public CWDMigraDocService(ICWDStorageRepository cWDStorageRepository) : base()
        {
            this.cWDStorageRepository = cWDStorageRepository;
        }

        public override async Task<Document> CreateDocument(T pdfData)
        {
            var data = pdfData as CWDDocumentData;

            var constructor = new DocumentConstructor(data, cWDStorageRepository);

            return await constructor.Create();
        }
    }

    /// <summary>
    /// non generic class for hotreload in dotnet 6
    /// </summary>
    internal class DocumentConstructor
    {
        private const string bodyMarginLeft = "1.5cm";
        private const string MarginLeftRight = "1,5cm";
        private readonly string _imagesPath = ".\\Images";

        private readonly ICWDStorageRepository cWDStorageRepository;
        private CWDDocumentData data;
        private Document document;

        public DocumentConstructor(CWDDocumentData data, ICWDStorageRepository cWDStorageRepository)
        {
            this.document = new Document();
            document.DefaultPageSetup.LeftMargin = bodyMarginLeft;
            document.DefaultPageSetup.RightMargin = bodyMarginLeft;
            this.data = data;
            this.cWDStorageRepository = cWDStorageRepository;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
        }

        public async Task<Document> Create()
        {
            var bijlages = new List<Bijlage>();
            var bijlageNummering = 1;
            if (data.NOKAntwoorden != null && data.NOKAntwoorden.Any() && data.NOKAntwoorden.Select(e => e.Fotos.Any()).Any())
            {
                bijlages.Add(new Bijlage()
                {
                    Nummer = bijlageNummering++,
                    Titel = "NOKAntwoord bijlages",
                    BijlageItems = data.NOKAntwoorden.Select(e => new BijlageItem() { Tekst = e.VraagTekst, Fotos = e.Fotos.ToList().Select(f => f.FotoId) }),
                });
            }

            if (data.OverigeAntwoorden != null && data.OverigeAntwoorden.Any() && data.OverigeAntwoorden.Select(e => e.Fotos.Any()).Any())
            {
                bijlages.Add(new Bijlage()
                {
                    Nummer = bijlageNummering++,
                    Titel = "Overige antwoorden bijlages",
                    BijlageItems = data.OverigeAntwoorden.Select(e => new BijlageItem() { Tekst = e.VraagTekst, Fotos = e.Fotos.ToList().Select(f => f.FotoId) }),
                });
            }

            data.Bijlages = bijlages;

            DefineStyles();
            await DefineCover();
            await DefineMainContentSection();

            return document;
        }

        public async Task<string> FetchImageFromStorage(string reference)
        {
            var image = await cWDStorageRepository.GetFileFromStorage(reference);
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

        private async Task DefineCover()
        {
            //https://forum.pdfsharp.net/viewtopic.php?f=2&t=3555

            var section = document.AddSection();

            section.PageSetup.TopMargin = 0;
            section.PageSetup.BottomMargin = 0;
            section.PageSetup.LeftMargin = 0;
            section.PageSetup.RightMargin = 0;
            var image = section.AddImage(await FetchImageFromStorage("CoverImage"));
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.RelativeVertical = RelativeVertical.Paragraph;
            image.Left = 0;
            image.Width = "21cm";
            image.Height = "29,6cm";
            image.Top = 0;

            image = section.AddImage(await FetchImageFromStorage("CWDInverse"));
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.RelativeVertical = RelativeVertical.Page;
            image.Left = MarginLeftRight;
            image.Width = "11cm";
            image.Top = 0;

            var tf = section.AddTextFrame();
            tf.Left = MarginLeftRight;
            tf.Top = "8cm";
            tf.Width = "16cm";
            // tf.LineFormat = new LineFormat() { DashStyle = DashStyle.Solid, Color =
            // Color.FromRgb(255, 0, 0), Width = "1pt" };
            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;
            var p = tf.AddParagraph();

            p.Format.Alignment = ParagraphAlignment.Left;
            p.Style = "Normal";
            p.Format.Font.Size = "18pt";
            p.Format.Font.Color = Colors.White;
            p.AddFormattedText($"Inspectie Rapport", new Font { Size = "42pt" });
            p.AddLineBreak();
            p.AddFormattedText($"{data.Klant}", new Font { Size = "42pt", Bold = true });
            p.AddLineBreak();
            p.AddLineBreak();
            p.AddFormattedText($"{data.InspectieTemplateNaam} - {data.InspectieNummer}", new Font { Size = "18pt" });

            tf = section.AddTextFrame();
            tf.Left = MarginLeftRight;
            tf.Top = "25cm";
            tf.Width = "10cm";
            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;

            p = tf.AddParagraph();
            p.Style = "Normal";
            p.Format.Font.Size = "14pt";
            p.Format.Font.Color = Colors.White;
            p.Style = "Normal";
            p.AddFormattedText("Datum van Inspectie:", new Font() { Bold = true });
            p.AddLineBreak();
            p.AddText(data.InspectieDatum.Value.ToShortDateString());

            tf = section.AddTextFrame();
            tf.Left = "12cm";
            tf.Top = "25cm";
            tf.Width = "10cm";
            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;

            p = tf.AddParagraph();
            p.Style = "Normal";
            p.Format.Font.Size = "14pt";
            p.Format.Font.Color = Colors.White;
            p.AddFormattedText("Inspecteur:", new Font() { Bold = true });
            p.AddLineBreak();
            p.AddText(data.Inspecteur);
        }

        private async Task DefineMainContentSection()
        {
            await InjectHeader();
            InjectFooter();
            document.LastSection.PageSetup.LeftMargin = MarginLeftRight;
            document.LastSection.PageSetup.RightMargin = MarginLeftRight;
            document.LastSection.PageSetup.TopMargin = "3cm";
            document.LastSection.PageSetup.BottomMargin = "3cm";
            InjectQuestions();
            InjectGeneralRemarks();
            document.LastSection.AddPageBreak();
            InjectOtherQuestions();
            document.LastSection.AddPageBreak();
            await InjectAddemdums();
            await InjectSignature();
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

        private async Task InjectAddemdums()
        {
            if (data.Bijlages == null || !data.Bijlages.Any())
            {
                return;
            }

            foreach (var bijlage in data.Bijlages)
            {
                var paragraph = document.LastSection.AddParagraph();
                paragraph.Style = "Header";
                paragraph.AddText($"Bijlage {bijlage.Nummer}: {bijlage.Titel}");

                foreach (var item in bijlage.BijlageItems)
                {
                    paragraph = document.LastSection.AddParagraph();
                    paragraph.Style = "QuestionHeader";
                    paragraph.AddFormattedText(item.Tekst);
                    paragraph.AddLineBreak();
                    foreach (var reference in item.Fotos)
                    {
                        var image = paragraph.AddImage(await FetchOptionalImage(reference.ToString()));
                        image.Width = "150pt";
                        image.LockAspectRatio = true;
                    }
                    paragraph.AddLineBreak();
                }

                if (bijlage.BijlageItems.Any())
                    document.LastSection.AddPageBreak();
            }
        }

        private void InjectFooter()
        {
            var MainSection = document.Sections[0];
            var paragraph = MainSection.Footers.Primary.AddParagraph();

            paragraph.Style = "Footer";
            paragraph.AddPageField();
            paragraph.AddTab();
            paragraph.AddFormattedText("Intelligentie door technologie", new Font() { Color = Colors.LightGray, Size = "14pt" });
        }

        private void InjectGeneralRemarks()
        {
            var paragraph = document.LastSection.AddParagraph();

            paragraph.Style = "Header";
            paragraph.AddText("Algemene Opmerkingen");
            paragraph = document.LastSection.AddParagraph();

            paragraph.Style = "QuestionBody";
            paragraph.AddText(data.Opmerkingen);
        }

        private async Task InjectHeader()
        {
            var section = document.AddSection();
            var p = section.Headers.Primary.AddParagraph();

            p.Format.Alignment = ParagraphAlignment.Left;
            p.Style = "Normal";
            p.Format.Font.Size = "18pt";
            p.AddFormattedText($"{data.InspectieTemplateNaam}");
            p.AddLineBreak();
            p.Format.Font.Size = "11pt";
            p.AddFormattedText($"Project: {data.InspectieNummer}");
            p.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };

            var tf = section.Headers.Primary.AddTextFrame();

            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;
            tf.Top = ShapePosition.Top;
            tf.Left = ShapePosition.Right;
            var paragraph = tf.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Right;

            var image = paragraph.AddImage(await FetchImageFromStorage("cwdlogo"));
            image.Width = "150pt";
            p.Format.SpaceAfter = "4cm";
        }

        private void InjectOtherQuestions()
        {
            var paragraph = document.LastSection.AddParagraph();

            paragraph.Style = "Header";
            paragraph.AddText("Overige vragen");
            foreach (var item in data.OverigeAntwoorden)
            {
                paragraph = document.LastSection.AddParagraph();
                paragraph.Style = "QuestionHeader";
                paragraph.AddText(item.VraagTekst);

                paragraph = document.LastSection.AddParagraph();
                paragraph.Style = "QuestionBody";
                paragraph.AddText(item.Antwoord);
            }

            paragraph.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };
        }

        private void InjectQuestions()
        {
            var paragraph = document.LastSection.AddParagraph();
            paragraph.Style = "Header";
            paragraph.AddText("Inspectie Rapport");

            paragraph = document.LastSection.AddParagraph();
            paragraph.Style = "Normal";
            paragraph.AddText("Naar aanleiding van de uitgevoerde inspectie zijn onderstaande afwijkingen geconstateerd welke beschouwd zijn als “Niet ok”");
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();

            foreach (var item in data.NOKAntwoorden)
            {
                paragraph = document.LastSection.AddParagraph();
                paragraph.Style = "QuestionHeader";
                paragraph.AddText(item.VraagTekst);

                paragraph = document.LastSection.AddParagraph();
                paragraph.Style = "QuestionBody";
                paragraph.AddText($"\"{item.Antwoord}\"");
            }

            paragraph.AddLineBreak();
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.SpaceBefore = "2cm";
            paragraph.Format.Borders = new Borders() { Bottom = new Border() { Color = Colors.DarkGray, Width = "2pt" } };
        }

        private async Task InjectSignature()
        {
            var p = document.LastSection.AddParagraph();
            p.Format.SpaceBefore = "2cm";
            p.Format.KeepTogether = true;
            p.Format.Font.Size = "10pt";
            p.AddText("Ondergetekend");
            p.AddLineBreak();
            p.AddText(data.Inspecteur);
            p.AddLineBreak();
            var datum = data.InspectieDatum.HasValue ? data.InspectieDatum.Value.ToShortDateString() : "onbekend";
            p.AddText($"Datum : {datum}");
            p.AddLineBreak();
            p.AddText($"Handtekening");
            p.AddLineBreak();
            var base64Image = await FetchOptionalImage(data.HandtekeningInspecteurId.ToString());
            var image = p.AddImage(base64Image);
            image.Width = "100pt";
            image.LockAspectRatio = true;
            p.AddLineBreak();
            p.AddFormattedText(@"N.B. Dit rapport mag slechts in zijn geheel zonder enige toevoegingen of weglatingen gepubliceerd worden. Voor afwijkingen van deze voorwaarden of voor publicatie in vertaling is schriftelijk toestemming vereist van Croonwolterendros B.V. Onafhankelijk van de inhoud van dit rapport aanvaardt de afdeling Energie van Croonwolterendros B.V. geen enkele aansprakelijkheid ten aanzien van de installatie.", new Font() { Size = "8pt" });
            p.AddLineBreak();
            p.AddLineBreak();
            p.AddFormattedText("De inspecteur, een vakbekwame persoon die deskundig is in het inspecteren van elektrotechnische laagspanningsinstallaties en door Croonwolterendros B.V. aangewezen tot het uitvoeren van NEN 3140/NEN3840 inspecties, verklaart dat de inspectie conform het opgestelde inspectieplan is uitgevoerd en bevindingen van de uitgevoerde inspectie op de juiste wijze in deze rapportage zijn weergegeven.", new Font() { Size = "8pt" });
            p.AddLineBreak();
            p.AddLineBreak();
            p.AddLineBreak();
        }
    }
}