using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using ThorstenHans.XmasTagger;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ThorstenHans.XmasTagger
{

    public class Startup : FunctionsStartup
    {
        private static IConfiguration _configuration = null;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<XmasTaggerConfig>()
                .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(XmasTaggerConfig.SectionName).Bind(settings);
            });
        }
    }
}
