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
    internal static class ProviderExtensions
    {
        public static string ToPrettyString(this IConfigurationProvider provider)
        {
            var s = provider.ToString();
            var typeName = provider.GetType().ToString();

            return s != typeName ? s : provider.GetType().Name;
        }
    }
    
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

        public override int Execute(CommandContext context, Settings settings)
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
            var tree = new Tree("ASP.NET Configuration");
            RecurseChildren(tree, null, root.GetChildren(), root, settings.ShowEnvironmental);


            AnsiConsole.Render(tree);

            return 0;
        }

        private static void RecurseChildren(Tree rootTree, TreeNode parentNode,
            IEnumerable<IConfigurationSection> children, IConfigurationRoot root, bool showEnvironmental)
        {
            TreeNode envNode = null;
            if (parentNode == null)
            {
                envNode = new TreeNode(new Markup("[yellow]Environment Variables[/]"));
            }

            foreach (var child in children)
            {
                var (value, provider) = GetValueAndProvider(root, child.Path);

                var name = provider != null
                    ? $"[blue]{child.Key}[/][grey]=[/][yellow]{value}[/] [grey]{provider.ToPrettyString()}[/]"
                    : $"[green]{child.Key}[/]";

                var newNode = parentNode switch
                {
                    null when provider != null && provider.ToString() == "EnvironmentVariablesConfigurationProvider" =>
                        envNode.AddNode(name),
                    null => rootTree.AddNode(name),
                    _ => parentNode.AddNode(name)
                };

                foreach (var configurationProvider in root.Providers)
                {
                    if (provider == null || configurationProvider.ToString() == provider.ToString()) continue;

                    if (configurationProvider.TryGet(child.Path, out var overridenValue))
                    {
                        newNode.AddNode($"[strikethrough]{overridenValue}[/] : [grey]{configurationProvider.ToPrettyString()}[/]");
                    }
                }

                RecurseChildren(rootTree, newNode, child.GetChildren(), root, showEnvironmental);
            }

            if (showEnvironmental && envNode != null) rootTree.AddNode(envNode);
        }

        private static (string Value, IConfigurationProvider Provider) GetValueAndProvider(IConfigurationRoot root,
            string key)
        {
            foreach (var provider in root.Providers.Reverse())
            {
                if (provider.TryGet(key, out var value))
                {
                    return (value, provider);
                }
            }

            return (null, null);
        }
    }
}