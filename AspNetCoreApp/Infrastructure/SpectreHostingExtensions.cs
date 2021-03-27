using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace AspNetCoreApp.Infrastructure
{
    static partial class SpectreHostingExtensions
    {
        public static Task<int> RunSpectreCommands(this IHostBuilder hostBuilder, string[] args)
        {
            var registrations = new ServiceCollection();
            registrations.AddSingleton(hostBuilder);
            var registrar = new TypeRegistrar(registrations);
            var app = new CommandApp<DefaultRunner>(registrar);

            app.Configure(config =>
            {
                config.AddCommand<DebugView>("debugview");
            });

            return app.RunAsync(args);
        }
    }
}