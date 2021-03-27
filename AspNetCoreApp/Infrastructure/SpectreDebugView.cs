using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Spectre.Console;

namespace AspNetCoreApp.Infrastructure
{
    internal static class SpectreDebugView
    {
        public static Tree GetDebugViewTree(this IConfigurationRoot root, string headingText,
            bool includeEnvironmentalVariables)
        {
            var tree = new Tree(headingText);
            RecurseChildren(tree, null, root.GetChildren(), root, includeEnvironmentalVariables);
            return tree;
        }

        private static string ToPrettyString(this IConfigurationProvider provider)
        {
            // some providers ToString() implementation is to return the entire type name, namespace
            // included, which gets kinda long. if they aren't doing anything special return just the type name
            var s = provider.ToString();
            var typeName = provider.GetType().ToString();

            return s != typeName ? s : provider.GetType().Name;
        }

        private static void RecurseChildren(Tree rootTree, TreeNode parentNode,
            IEnumerable<IConfigurationSection> children, IConfigurationRoot root, bool showEnvironmental)
        {
            TreeNode envNode = null;
            if (parentNode == null)
            {
                envNode = new TreeNode(new Markup("[green]Environment Variables[/]"));
            }

            foreach (var child in children)
            {
                var results = GetValueAndProvider(root, child.Path);
                var (value, provider) = results?.Pop() ?? (null, null);

                var name = results != null
                    ? $"[blue]{child.Key}[/][grey]=[/][yellow]{value}[/] [grey]{provider.ToPrettyString()}[/]"
                    : $"[green]{child.Key}[/]";

                // if there is a parent node then just add this item to that parent. if not then we are at the root.
                // In that case let's check to see if we are dealing with an environmental variable with no other value and if so we'll add 
                // it to the environmental tree node list
                var newNode = parentNode is not null
                    ? parentNode.AddNode(name)
                    : provider?.GetType() == typeof(EnvironmentVariablesConfigurationProvider) && results?.Count == 0
                        ? envNode.AddNode(name)
                        : rootTree.AddNode(name);

                // loop through the providers again looking to see if there are any other values that might have been overriden
                while (results != null && results.Count > 0)
                {
                    var (overridenValue, configurationProvider) = results.Pop();
                    newNode.AddNode(
                        $"[strikethrough]{overridenValue}[/] : [grey]{configurationProvider.ToPrettyString()}[/]");
                }

                RecurseChildren(rootTree, newNode, child.GetChildren(), root, showEnvironmental);
            }

            if (showEnvironmental && envNode != null) rootTree.AddNode(envNode);
        }

        private static Stack<(string Value, IConfigurationProvider Provider)> GetValueAndProvider(
            IConfigurationRoot root,
            string key)
        {
            var results = new Stack<(string Value, IConfigurationProvider Provider)>();
            foreach (var provider in root.Providers)
            {
                if (provider.TryGet(key, out var value))
                {
                    results.Push((value, provider));
                }
            }

            return results.Count == 0 ? null : results;
        }
    }
}