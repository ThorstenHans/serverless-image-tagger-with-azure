using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ThorstenHans.ImageTagger.Startup))]
namespace ThorstenHans.ImageTagger
{

    public class Startup : FunctionsStartup
    {
        private static IConfiguration _configuration = null;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<ImageTaggerConfig>()
                .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection(ImageTaggerConfig.SectionName).Bind(settings);
            });
        }
    }
}
