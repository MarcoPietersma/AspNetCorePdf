using Macaw.Pdf.Model;
using MigraDoc.DocumentObjectModel;

namespace Macaw.Pdf
{
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

                DefineStyles(document);

                DefineCover(document);

                DefineContentSection(document);

                return document;
            }
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

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called TextBox based on style Normal
            style = document.Styles.AddStyle("TextBox", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Distance = "3pt";
            style.ParagraphFormat.Shading.Color = Colors.SkyBlue;

            // Create a new style called TOC based on style Normal
            style = document.Styles.AddStyle("TOC", "Normal");
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
            style.ParagraphFormat.Font.Color = Colors.Blue;
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

            var paragraph = section.AddParagraph();
            paragraph.Format.SpaceAfter = "3cm";

            var image = section.AddImage($"{_imagesPath}\\CoverImage.jpg");
            image.Width = "21cm";
            image.Top = 0;

            paragraph = section.AddParagraph("A sample document that demonstrates the\ncapabilities of MigraDoc");
            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Color = Colors.DarkRed;
            paragraph.Format.SpaceBefore = "8cm";
            paragraph.Format.SpaceAfter = "3cm";

            paragraph = section.AddParagraph("Rendering date: ");
            paragraph.AddDateField();
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
}