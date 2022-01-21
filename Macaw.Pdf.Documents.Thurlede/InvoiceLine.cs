namespace Macaw.Pdf
{
    public class InvoiceLine
    {
        public string Description { get; set; }
        public int NumberOfItems { get; set; }
        public decimal PricePerItem { get; set; }

        public decimal Total
        { get { return PricePerItem * NumberOfItems; } }
    }
}