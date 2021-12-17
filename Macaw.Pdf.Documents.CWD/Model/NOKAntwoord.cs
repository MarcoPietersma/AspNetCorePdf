using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class NOKAntwoord
    {
        public string Antwoord { get; set; }
        public IEnumerable<Foto> Fotos { get; set; }
        public string VraagTekst { get; set; }
    }
}