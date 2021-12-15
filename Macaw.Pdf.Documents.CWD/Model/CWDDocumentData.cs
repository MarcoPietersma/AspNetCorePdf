using Macaw.Pdf.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class CWDDocumentData : IPdfData
    {
        public IEnumerable<Bijlage> Bijlages { get; set; }
        public string DocumentName { get; set; }

        [JsonProperty("e-mailmanager")]
        public string EmailManager { get; set; }

        [JsonProperty("e-mailrapport")]
        public string EmailRapport { get; set; }

        public Guid? HandtekeningInspecteurId { get; set; }
        public Guid? HandtekeningManagerId { get; set; }
        public string Inspecteur { get; set; }
        public DateTime InspectieDatum { get; set; }
        public string InspectieNummer { get; set; }

        public string InspectieTemplateNaam { get; set; }

        public string Klant { get; set; }

        public string Manager { get; set; }

        [JsonIgnore]
        public IEnumerable<NOKAntwoord> NOKAntwoorden { get; set; }

        [JsonProperty("NOKAntwoorden")]
        public string NOKAntwoordenString { get; set; }

        public string ObjectNummer { get; set; }

        public string Opmerkingen { get; set; }

        [JsonProperty("otherAntwoorden")]
        public string OtherAntwoordenString { get; set; }

        [JsonIgnore]
        public IEnumerable<OverigAntwoord> OverigeAntwoorden { get; set; }
    }
}