namespace Macaw.Pdf.Model
{
    public class DemoDocumentData : IPdfData
    {
        public string CreatedBy
        {
            get; set;
        }

        public string Description
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