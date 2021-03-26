using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

namespace AspNetCoreApp.Infrastructure
{
    internal class HostBuilderIntercept : ICommandInterceptor
    {
        private readonly IHostBuilder _runtimeSource;

        public HostBuilderIntercept(IHostBuilder runtimeSource)
        {
            _runtimeSource = runtimeSource;
        }

        public void Intercept(CommandContext context, CommandSettings settings)
        {
            if (settings is IHostBuilderInput input)
            {
                input.HostBuilder = _runtimeSource;
            }
        }
    }
}