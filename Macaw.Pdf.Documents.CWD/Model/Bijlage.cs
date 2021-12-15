using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class Bijlage
    {
        public IEnumerable<BijlageItem> BijlageItems { get; set; }
        public int Nummer { get; set; }
        public string Titel { get; set; }
    }
}