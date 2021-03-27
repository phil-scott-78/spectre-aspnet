using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AspNetCoreApp.Infrastructure
{
    internal class DebugView : Command<DebugView.Settings>
    {
        private readonly IHostBuilder _hostBuilder;

        public DebugView(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public class Settings : CommandSettings
        {
            [Description("Show environmental variables")]
            [CommandOption("--env")]
            public bool ShowEnvironmental { get; set; }
        }

        public override int Execute(CommandContext context, DebugView.Settings settings)
        {
            using var host = _hostBuilder.Build();
            var root = (IConfigurationRoot) host.Services.GetService<IConfiguration>();
            if (root == null) throw new Exception("Unable to resolve IConfiguration");

            AnsiConsole.Render(new Rule("Provider Order"));

            var counter = 1;
            foreach (var configurationProvider in root.Providers)
            {
                AnsiConsole.WriteLine($"{counter++}. {configurationProvider.ToString() ?? ""}");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("Config"));
            var tree = root.GetDebugViewTree("ASP.NET Configuration", settings.ShowEnvironmental);
            AnsiConsole.Render(tree);

            return 0;
        }
    }
}