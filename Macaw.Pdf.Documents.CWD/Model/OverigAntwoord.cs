using System;
using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class Foto
    {
        public Guid FotoId { get; set; }
    }

    public class OverigAntwoord
    {
        public string Antwoord { get; set; }
        public IEnumerable<Foto> Fotos { get; set; }
        public string VraagTekst { get; set; }
    }
}