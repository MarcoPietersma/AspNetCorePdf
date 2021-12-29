namespace Macaw.Pdf.Storage
{
    using Macaw.Pdf.Interfaces;
    using Microsoft.Extensions.Configuration;

    public class CWDStorageRepository : StorageRepository, ICWDStorageRepository
    {
        public CWDStorageRepository(IConfiguration configuration) : base(configuration)
        {
            ContainerName = "CWD";
        }
    }
}