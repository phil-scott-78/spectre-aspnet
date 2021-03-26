using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AspNetCoreApp.Infrastructure
{
    static partial class SpectreHostingExtensions
    {
        public static Task<int> RunSpectreCommands(this IHostBuilder hostBuilder, string[] args)
        {
            var app = new CommandApp<DefaultRunner>();
            DefaultRunner.HostBuilder = hostBuilder;

            app.Configure(config =>
            {
                config.SetInterceptor(new HostBuilderIntercept(hostBuilder));
                config.AddCommand<DebugView>("debugview");
                config.AddDelegate("", ctx =>
                {
                    hostBuilder.Build().StartAsync();
                    return 0;
                });
            });

            return app.RunAsync(args);
        }
    }
}