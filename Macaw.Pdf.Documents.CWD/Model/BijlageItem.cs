using System;
using System.Collections.Generic;

namespace Macaw.Pdf.Documents.CWD
{
    public class BijlageItem
    {
        public IEnumerable<Guid> Fotos { get; set; }
        public string Tekst { get; set; }
    }
}