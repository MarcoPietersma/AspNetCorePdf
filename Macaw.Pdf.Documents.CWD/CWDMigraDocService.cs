using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Macaw.Pdf;

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
    private const string marginLeft = "3cm";
    private readonly string _imagesPath = ".\\Images";

    private readonly ICWDStorageRepository cWDStorageRepository;
    private CWDDocumentData data;
    private Document document;

    public DocumentConstructor(CWDDocumentData data, ICWDStorageRepository cWDStorageRepository)
    {
        this.document = new Document();
        this.data = data;
        this.cWDStorageRepository = cWDStorageRepository;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
    }

    public async Task<Document> Create()
    {
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
        // Get the predefined style Normal.
        var style = document.Styles["Normal"];
        // Because all styles are derived from Normal, the next line changes the font of the whole
        // document. Or, more exactly, it changes the font of all styles and paragraphs that do not
        // redefine the font.
        style.Font.Name = "OpenSans";

        // Heading1 to Heading9 are predefined styles with an outline level. An outline level other
        // than OutlineLevel.BodyText automatically creates the outline (or bookmarks) in PDF.

        style = document.Styles["Heading1"];
        style.Font.Size = 14;
        style.Font.Bold = true;
        style.Font.Color = Colors.DarkBlue;
        style.ParagraphFormat.PageBreakBefore = true;
        style.ParagraphFormat.SpaceAfter = 6;

        style = document.Styles["Heading2"];
        style.Font.Size = 12;
        style.Font.Bold = true;
        style.ParagraphFormat.PageBreakBefore = false;
        style.ParagraphFormat.SpaceBefore = 6;
        style.ParagraphFormat.SpaceAfter = 6;

        style = document.Styles["Heading3"];
        style.Font.Size = 10;
        style.Font.Bold = true;
        style.Font.Italic = true;
        style.ParagraphFormat.SpaceBefore = 6;
        style.ParagraphFormat.SpaceAfter = 3;

        document.Styles.AddStyle("QuestionHeader", "Normal");
        document.Styles.AddStyle("QuestionBody", "Normal");

        style = document.Styles["QuestionHeader"];
        style.Font.Size = 10;
        style.Font.Bold = true;
        style.Font.Italic = true;
        style.ParagraphFormat.SpaceBefore = 1;
        style.ParagraphFormat.SpaceAfter = 2;

        style = document.Styles["QuestionBody"];
        style.Font.Size = 10;
        style.Font.Bold = true;
        style.Font.Italic = true;
        style.ParagraphFormat.SpaceBefore = 1;
        style.ParagraphFormat.SpaceAfter = 2;
    }

    private void DefineContentSection()
    {
        var section = document.AddSection();
        section.PageSetup.OddAndEvenPagesHeaderFooter = true;
        section.PageSetup.StartingNumber = 1;

        var header = section.Headers.Primary;
        header.AddParagraph("\tOdd Page Header");

        header = section.Headers.EvenPage;
        header.AddParagraph("Even Page Header");

        // Create a paragraph with centered page number. See definition of style "Footer".
        var paragraph = new Paragraph();
        paragraph.AddTab();
        paragraph.AddPageField();

        // Add paragraph to footer for odd pages.
        section.Footers.Primary.Add(paragraph);
        // Add clone of paragraph to footer for odd pages. Cloning is necessary because an object
        // must not belong to more than one other object. If you forget cloning an exception is thrown.
        section.Footers.EvenPage.Add(paragraph.Clone());
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
        image.Left = marginLeft;
        image.Width = "11cm";
        image.Top = 0;

        var tf = section.AddTextFrame();
        tf.Left = marginLeft;
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
        p.AddFormattedText($"Inspectie Raport", new Font { Size = "42pt" });
        p.AddLineBreak();
        p.AddFormattedText($"{data.Klant}", new Font { Size = "42pt", Bold = true });
        p.AddLineBreak();
        p.AddLineBreak();
        p.AddFormattedText($"{data.InspectieTemplateNaam} - {data.InspectieNummer}", new Font { Size = "18pt" });

        tf = section.AddTextFrame();
        tf.Left = marginLeft;
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
        p.AddText(data.InspectieDatum.ToShortDateString());

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
        InjectQuestions();
        InjectGeneralRemarks();
        InjectOtherQuestions();
        await InjectAddemdums();
    }

    private async Task InjectAddemdums()
    {
        if (data.Bijlages == null)
        {
            return;
        }
        foreach (var bijlage in data.Bijlages)
        {
            var paragraph = document.LastSection.AddParagraph();

            paragraph.AddFormattedText($"Bijlage {bijlage.Nummer}: {bijlage.Titel}", new Font { Size = "21pt" });
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();

            foreach (var item in bijlage.BijlageItems)
            {
                paragraph.AddFormattedText(item.Tekst, new Font() { Bold = true });
                paragraph.AddLineBreak();
                foreach (var reference in item.Fotos)
                {
                    var base64Image = await FetchImageFromStorage(reference);
                    var image = paragraph.AddImage(base64Image);
                    image.Width = 300;
                    image.LockAspectRatio = true;
                }
                paragraph.AddLineBreak();
            }
        }
    }

    private void InjectGeneralRemarks()
    {
        var paragraph = document.LastSection.AddParagraph();

        paragraph.Format.Font.Size = "21pt";
        paragraph.AddText("Algemene Opmerkingen");

        paragraph = document.LastSection.AddParagraph();

        paragraph.Format.Alignment = ParagraphAlignment.Left;
        paragraph.Style = "QuestionHeader";
        paragraph.AddText(data.Opmerkingen);
        paragraph.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };
    }

    private async Task InjectHeader()
    {
        var section = document.AddSection();

        var p = section.Headers.Primary.AddParagraph();

        p.Format.Alignment = ParagraphAlignment.Left;
        p.Style = "Normal";
        p.Format.Font.Size = "18pt";
        p.AddFormattedText($"NEN {data.InspectieNummer}, Inspectie raport :{data.DocumentName}");
        p.AddLineBreak();
        p.Format.Font.Size = "11pt";
        p.AddFormattedText($"Project: {data.ObjectNummer} PO no:{data.ObjectNummer}");
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
    }

    private void InjectOtherQuestions()
    {
        var paragraph = document.LastSection.AddParagraph();

        paragraph.Format.Font.Size = "21pt";
        paragraph.AddText("Overige vragen");
        paragraph.Format.SpaceAfter = "2cm";
        foreach (var item in data.OverigeAntwoorden)
        {
            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.Font.Bold = true;
            paragraph.Style = "QuestionHeader";
            paragraph.AddText(item.VraagTekst);

            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.SpaceAfter = "2cm";
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Style = "QuestionBody";
            paragraph.AddText(item.Antwoord);
        }

        paragraph.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };
    }

    private void InjectQuestions()
    {
        var paragraph = document.LastSection.AddParagraph();

        paragraph.Format.Font.Size = "21pt";
        paragraph.AddText("Niet Oke(andere tekst)");
        paragraph.Format.SpaceAfter = "2cm";

        foreach (var item in data.NOKAntwoorden)
        {
            paragraph = document.LastSection.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.Font.Bold = true;
            paragraph.Style = "QuestionHeader";
            paragraph.AddText(item.Vraag);

            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.SpaceAfter = "2cm";
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Style = "QuestionBody";
            paragraph.AddText(item.Antwoord);
        }

        paragraph.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };
    }
}