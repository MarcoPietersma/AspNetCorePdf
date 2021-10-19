using Macaw.Pdf.Model;
using System.Collections.Generic;

namespace Macaw.Pdf.Interfaces
{
    public interface IPdfData
    {
        string CreatedBy
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        List<ItemsToDisplay> DisplayListItems
        {
            get;
            set;
        }

        string DocumentName
        {
            get;
            set;
        }

        string DocumentTitle
        {
            get;
            set;
        }
    }
}