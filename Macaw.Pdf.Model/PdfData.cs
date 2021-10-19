using System.Collections.Generic;

namespace Macaw.Pdf.Model
{
    public abstract class PdfData : IPdfData
    {
        public string CreatedBy
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public List<ItemsToDisplay> DisplayListItems
        {
            get; set;
        }

        public string DocumentName
        {
            get; set;
        }

        public string DocumentTitle
        {
            get; set;
        }
    }
}