using System.Collections;
using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class NOKAntwoord
    {
        public string Vraag { get; set; }
        public string Antwoord { get; set; }
        public IEnumerable<string> Fotos { get; set; }
    }
}