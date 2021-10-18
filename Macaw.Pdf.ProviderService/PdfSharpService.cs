﻿using System;
using System.IO;
using Macaw.Pdf.Model;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace Macaw.Pdf
{
    public class PdfSharpService : IPdfSharpService
    {
        private readonly string _createdDocsPath = ".\\PdfProvider\\Created";
        private readonly string _imagesPath = ".\\PdfProvider\\Images";
        private readonly string _resourcesPath = ".\\PdfProvider\\Resources";

        public string CreatePdf(PdfData pdfData)
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new FontResolver(_resourcesPath);
            }

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            AddTitleLogo(gfx, page, $"{_imagesPath}\\logo.jpg", 0, 0);
            AddTitleAndFooter(page, gfx, pdfData.DocumentTitle, document, pdfData);
            AddDescription(gfx, pdfData);
            AddList(gfx, pdfData);

            string docName = $"{_createdDocsPath}/{pdfData.DocumentName}-{DateTime.UtcNow.ToOADate()}.pdf";
            document.Save(docName);
            return docName;
        }

        private void AddDescription(XGraphics gfx, PdfData pdfData)
        {
            XFont font = new XFont("OpenSans", 14, XFontStyle.Regular);
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect rect = new XRect(40, 100, 520, 100);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(pdfData.Description, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        private void AddList(XGraphics gfx, PdfData pdfData)
        {
            int startingHeight = 200;
            int listItemHeight = 30;

            for (int i = 0; i < pdfData.DisplayListItems.Count; i++)
            {
                XFont font = new XFont("OpenSans", 14, XFontStyle.Regular);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(60, startingHeight, 500, listItemHeight);
                gfx.DrawRectangle(XBrushes.White, rect);
                string data = $"{i}. {pdfData.DisplayListItems[i].Id} | {pdfData.DisplayListItems[i].Data1} | {pdfData.DisplayListItems[i].Data2}";
                tf.DrawString(data, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                startingHeight = startingHeight + listItemHeight;
            }
        }

        private void AddTitleAndFooter(PdfPage page, XGraphics gfx, string title, PdfDocument document, PdfData pdfData)
        {
            XRect rect = new XRect(new XPoint(), gfx.PageSize);
            rect.Inflate(-10, -15);
            XFont font = new XFont("OpenSans", 14, XFontStyle.Bold);
            gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);

            rect.Offset(0, 5);
            font = new XFont("OpenSans", 8, XFontStyle.Italic);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Far;
            gfx.DrawString("Created by " + pdfData.CreatedBy, font, XBrushes.DarkOrchid, rect, format);

            font = new XFont("OpenSans", 8);
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(document.PageCount.ToString(), font, XBrushes.DarkOrchid, rect, format);

            document.Outlines.Add(title, page, true);
        }

        private void AddTitleLogo(XGraphics gfx, PdfPage page, string imagePath, int xPosition, int yPosition)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException(String.Format("Could not find image {0}.", imagePath));
            }

            XImage xImage = XImage.FromFile(imagePath);
            gfx.DrawImage(xImage, xPosition, yPosition, xImage.PixelWidth / 8, xImage.PixelHeight / 8);
        }
    }
}