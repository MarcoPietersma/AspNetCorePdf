using Macaw.Pdf.Interfaces;
using System;
using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class CWDDocumentData : IPdfData
    {
        public int HandtekeningInspecteurId { get; set; }
        public int HandtekeningManagerId { get; set; }

        public DateTime InspectieDatum { get; set; }

        public string InspectieNummer { get; set; }

        public string InspectieTemplateNaam { get; set; }

        public string Klant { get; set; }

        public string Manager { get; set; }
        public IEnumerable<NOKAntwoord> NOKAntwoorden { get; set; }
        public string ObjectNummer { get; set; }
        public IEnumerable<OverigAntwoord> OverigeAntwoorden { get; set; }
    }
}