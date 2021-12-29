namespace Macaw.Pdf.Storage
{
    using Macaw.Pdf.Interfaces;
    using Microsoft.Extensions.Configuration;

    public class ThurledeStorageRepository : StorageRepository, IThurledeStorageRepository
    {
        public ThurledeStorageRepository(IConfiguration configuration) : base(configuration)
        {
            ContainerName = "thurlede";
        }
    }
}