using Macaw.Pdf.Documents.CWD;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Macaw.Pdf.Startup))]

namespace Macaw.Pdf
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddScoped<IPdfSharpService, PdfSharpService>();
            services.AddScoped<IMigraDocService<SomeReport>, CWDMigraDocService<SomeReport>>();
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