using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using System;

namespace Macaw.Pdf;

public class CWDMigraDocService<T> : MigraDocService<T> where T : IPdfData
{
    private readonly string _imagesPath = ".\\Images";


    public override Document CreateDocument(T pdfData)
    {
        {
            // Create a new MigraDoc document
            var document = new Document();
            //document.Info.Title = pdfData.DocumentTitle;
            //document.Info.Subject = pdfData.Description;
            //document.Info.Author = pdfData.CreatedBy;

            var data = pdfData as CWDDocumentData;
            

            var constructor = new documentConstructor(this.data);

            return document;
        }
    }
}
    internal class documentConstructor 
{

    CWDDocumentData data;
    Document document;

    public documentConstructor(CWDDocumentData data)
    {
        this.data=data;
    }

    public Document Create()
    {

        DefineStyles();

        DefineCover();

        DefineMainContentSection();

    }

    private void DefineMainContentSection()
    {
        InjectHeader();
        InjectQuestions();
        InjectGeneralRemarks();
        InjectOtherQuestions();
        InjectAddemdums();
    }

    private void InjectAddemdums()
    {
       
    }

    private void InjectOtherQuestions()
    {
      
    }

    private void InjectGeneralRemarks()
    {
      
    }

    private void InjectQuestions()
    {
        foreach (var item in data.NOKAntwoorden)
        {            
            var paragraph = document.LastSection.AddParagraph();
         
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Style = "QuestionHeader";
            paragraph.AddText(item.Vraag);

            paragraph = document.LastSection.AddParagraph();
            paragraph.Format.SpaceAfter = "2cm";
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Style = "QuestionBody";
            paragraph.AddText(item.Antwoord);

        }
    }

    private void InjectHeader(Document document)
    {
        var section = document.AddSection();

        var p  =section.Headers.Primary.AddParagraph();

        p.Format.Alignment = ParagraphAlignment.Left;
        p.Style = "Normal";

        p.AddFormattedText($"NEN {data.InspectieNummer}, Inspectie raport :{data.DocumentName}");
        p.Format.Font.Size = 6;
        p.AddFormattedText($"Project: {data.ObjectNummer} PO no:{data.ObjectNummer}");
        p.Format.Borders.Bottom = new Border() { Color = Colors.DarkGray, Width = "1pt" };


    }

    internal void DefineStyles(Document document)
    {
        // Get the predefined style Normal.
        var style = document.Styles["Normal"];
        // Because all styles are derived from Normal, the next line changes the font of the
        // whole document. Or, more exactly, it changes the font of all styles and paragraphs
        // that do not redefine the font.
        style.Font.Name = "Segoe UI";

        // Heading1 to Heading9 are predefined styles with an outline level. An outline level
        // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) in PDF.

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
        style.ParagraphFormat.SpaceBefore = 6;
        style.ParagraphFormat.SpaceAfter = 3;

        style = document.Styles["QuestionBody"];
        style.Font.Size = 10;
        style.Font.Bold = true;
        style.Font.Italic = true;
        style.ParagraphFormat.SpaceBefore = 6;
        style.ParagraphFormat.SpaceAfter = 3;

    }

    private void DefineContentSection(Document document)
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
        // Add clone of paragraph to footer for odd pages. Cloning is necessary because an
        // object must not belong to more than one other object. If you forget cloning an
        // exception is thrown.
        section.Footers.EvenPage.Add(paragraph.Clone());
    }

    private void DefineCover(Document document)
    {
        var section = document.AddSection();

        var image = section.AddImage($"{_imagesPath}\\CoverImage.jpg");
        image.RelativeHorizontal = RelativeHorizontal.Page;
        image.RelativeVertical = RelativeVertical.Paragraph;
        image.Left = "0pt";
        image.Width = "21cm";
        image.Top = "0pt";

        
    }

    private void DefineParagraphs(Document document)
    {
        var paragraph = document.LastSection.AddParagraph("Paragraph Layout Overview", "Heading1");
        paragraph.AddBookmark("Paragraphs");
    }

    private void DefineTableOfContents(Document document)
    {
        var section = document.LastSection;

        section.AddPageBreak();
        var paragraph = section.AddParagraph("Table of Contents");
        paragraph.Format.Font.Size = 14;
        paragraph.Format.Font.Bold = true;
        paragraph.Format.SpaceAfter = 24;
        paragraph.Format.OutlineLevel = OutlineLevel.Level1;

        paragraph = section.AddParagraph();
        paragraph.Style = "TOC";
        var hyperlink = paragraph.AddHyperlink("Paragraphs");
        hyperlink.AddText("Paragraphs\t");
        hyperlink.AddPageRefField("Paragraphs");

        paragraph = section.AddParagraph();
        paragraph.Style = "TOC";
        hyperlink = paragraph.AddHyperlink("Tables");
        hyperlink.AddText("Tables\t");
        hyperlink.AddPageRefField("Tables");

        paragraph = section.AddParagraph();
        paragraph.Style = "TOC";
        hyperlink = paragraph.AddHyperlink("Charts");
        hyperlink.AddText("Charts\t");
        hyperlink.AddPageRefField("Charts");
    }

    private void DefineTables(Document document)
    {
        var paragraph = document.LastSection.AddParagraph("Table Overview", "Heading1");
        paragraph.AddBookmark("Tables");
    }
}
