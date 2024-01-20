﻿using System.ComponentModel;
using Microsoft.SemanticKernel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System;

namespace BlazorGPT.Shared.PluginSelector
{
    public class PluginsRepository
    {
        private IServiceProvider _serviceProvider;

        public PluginsRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<List<Plugin>> GetAll()
        {
            var internalNative = GetAllInternalNative();
            var semanticPlugins = await GetSemanticPlugins();
            var externalNative = GetExternalNative();

            var plugins = new List<Plugin>();
            foreach (var type in internalNative)
            {
                var plugin = new Plugin
                {
                    Name = type.Item1,
                    Functions = new List<Function>(),
                    IsNative = true
                };
                var f = new Function
                {
                    Name = type.Item2,
                    Description = type.Item3

                };
                plugin.Functions.Add(f);
                plugins.Add(plugin);
            }
 
            plugins.AddRange(externalNative);

            plugins.AddRange(semanticPlugins);

            return plugins;
        }

        public  async Task<List<Plugin>> GetAllInternal()
        {
            var internalNative = GetAllInternalNative();
            var semanticPlugins =  await  GetSemanticPlugins();

            var plugins = new List<Plugin>();
            foreach (var type in internalNative)
            {
                var plugin = new Plugin
                {
                    Name = type.Item1,
                    Functions = new List<Function>(),
                    IsNative = true
                };
                var f = new Function
                {
                    Name = type.Item2,
                    Description = type.Item3

                };
                plugin.Functions.Add(f);
                plugins.Add(plugin);
            }

            plugins.AddRange(semanticPlugins);
            return plugins;
        }

        public List<(string, string, string)> GetAllInternalNative()
        {
            Assembly assembly = Assembly.GetExecutingAssembly(); // Replace with the desired assembly

            var typesWithKernelFunctionAttribute = assembly.GetTypes()
                .Where(type => type.GetMethods()
                    .Any(method => method.GetCustomAttributes(typeof(KernelFunctionAttribute), true).Any()))
                .ToList();
            List<(string, string, string)> types = new List<(string, string, string)>();
            foreach (var type in typesWithKernelFunctionAttribute)
            {
                foreach (var method in type.GetMethods())
                {
                    var attr = method.GetCustomAttribute<KernelFunctionAttribute>();
                    if (attr != null)
                    {
                        var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                        string desc = descAttr?.Description ?? "";
                        // desc should be set to the DescriptionAttribute of the method

                        types.Add((type.FullName, method.Name, desc));
                    }
                }
            }

            return types;
        }

        // get all native plugins from all the assemblies in the Plugins subfolder and scan them for KernelFuntion methods. If these funtion methods are found, add a plugin to the list of plugins that is to be returned
        public List<Plugin> GetExternalNative()
        {
            var plugins = new List<Plugin>();
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var files = Directory.GetFiles(path, "*.dll");

            foreach (var file in files)
            {
                Assembly assembly = Assembly.LoadFile(file);
                var types = assembly.GetTypes()
                    .Where(type => type.GetMethods()
                                           .Any(method => method.GetCustomAttributes(typeof(KernelFunctionAttribute), true).Any()))
                    .ToList();


                foreach (var type in types)
                {
                    var instance = Activator.CreateInstance(type, _serviceProvider);
                    Plugin p = new Plugin() { Name = type.ToString(), IsNative = true };
                    p.Instance = instance;


                    var methods = type.GetMethods()
                        .Where(method => method.GetCustomAttributes(typeof(KernelFunctionAttribute), true).Any())
                        .ToList();

                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<KernelFunctionAttribute>();
                        if (attr != null)
                        {
                            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                            string desc = descAttr?.Description ?? "";
                            // desc should be set to the DescriptionAttribute of the method

                            var f = new Function
                            {
                                Name = method.Name,
                                Description = desc
                            };
                            p.Functions.Add(f);
                        }
                    }

                    plugins.Add(p);
                }
            }

            return plugins;
        }

        public async Task <List<Plugin>> GetSemanticPlugins()
        {
            var pluginsDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Plugins"));

            var plugins = new List<Plugin>();

            foreach (var pluginDirectory in pluginsDirectory.EnumerateDirectories())
            {
                var plugin = new Plugin
                {
                    Name = pluginDirectory.Name,
                    Functions = new List<Function>()
                };
                // iterate the plugin directories to get the functions and their descriptions
                foreach (var pluginFile in pluginDirectory.EnumerateDirectories())
                {
                    var configString = await File.ReadAllTextAsync(Path.Join(pluginFile.FullName, "config.json"));

                    SemanticPluginConfig?
                        config = JsonSerializer.Deserialize<SemanticPluginConfig>(configString);

                    var f = new Function
                    {
                        Name = pluginFile.Name,
                        Description = @config?.description

                    };
                    plugin.Functions.Add(f);
                }

                plugins.Add(plugin);
            }

            return plugins;
        }
    }
}
