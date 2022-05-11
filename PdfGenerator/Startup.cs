using Macaw.Pdf.Documents.CWD;
using Macaw.Pdf.Documents.Thurlede.Model;
using Macaw.Pdf.Interfaces;
using Macaw.Pdf.Model;
using Macaw.Pdf.Services;
using Macaw.Pdf.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Macaw.Pdf.Startup))]

namespace Macaw.Pdf
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var context = builder.GetContext();
            var services = builder.Services;

            var config = context.Configuration;

            // services.AddScoped<IPdfSharpService, DemoPdfSharpService>();
            services.AddScoped<IMigraDocService<DemoDocumentData>, DemoMigraDocService<DemoDocumentData>>();
            services.AddScoped<IMigraDocService<CWDDocumentData>, CWDMigraDocService<CWDDocumentData>>();
            services.AddScoped<IMigraDocService<ThurledeFactuur>, ThurledeMigraDocService<ThurledeFactuur>>();
            services.AddSingleton<ICWDStorageRepository, CWDStorageRepository>();
            services.AddSingleton<IThurledeStorageRepository, ThurledeStorageRepository>();

            services.AddScoped<ISendGridService, SendGridService>();
            services.AddSendGrid(e => { e.ApiKey = config["SendGridApiKey"]; });
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            var configBuilder = builder.ConfigurationBuilder
                .SetBasePath(context.ApplicationRootPath)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false);

            var config = configBuilder.Build();
        }
    }
}