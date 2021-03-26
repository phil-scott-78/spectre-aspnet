using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace AspNetCoreApp.Infrastructure
{
    static partial class SpectreHostingExtensions
    {
        public class DefaultRunner : AsyncCommand<DefaultRunner.Settings>
        {
            public class Settings : CommandSettings
            {
            }

            public static IHostBuilder HostBuilder { get; set; }

            public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
            {
                await HostBuilder.Build().RunAsync();
                return 0;
            }
        }
    }
}