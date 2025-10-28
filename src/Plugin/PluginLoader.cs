// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BlockIO.Interface;

#if BLOCKIO_PLUGIN_SUPPORT

namespace BlockIO.Plugin
{
    /// <summary>
    /// Loads BlockIO plugins from external assemblies that implement IBlockIOPlugin.
    /// </summary>
    /// <example>
    /// Example usage:
    /// <code>
    /// var loader = new PluginLoader();
    /// loader.LoadFromDirectory("plugins/");
    /// foreach (var plugin in loader.Plugins)
    /// {
    ///     Console.WriteLine($"Loaded plugin: {plugin.Name}");
    ///     foreach (var parser in plugin.GetParsers()) 
    ///         // Register parser
    ///     foreach (var parser in plugin.GetDevices()) 
    ///         // Register devices
    /// }
    /// </code>
    /// </example>
    public sealed class PluginLoader
    {
        public List<IBlockIOPlugin> Plugins { get; } = new();

        public PluginLoader()
        {
        }

        public void LoadFromDirectory(string path)
        {
            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.GetFiles(path, "*.dll"))
                Plugins.AddRange(LoadFromAssembly(file));
        }

        public List<IBlockIOPlugin> LoadFromAssembly(string assemblyPath)
        {
            var result = new List<IBlockIOPlugin>();
            try
            {
                var asm = Assembly.LoadFrom(assemblyPath);
                foreach (var type in asm.GetTypes())
                {
                    if (typeof(IBlockIOPlugin).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        if (Activator.CreateInstance(type) is IBlockIOPlugin plugin)
                            result.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Plugin load failed: {ex.Message}");
            }
            return result;
        }

        public void ClearPlugins()
        {
            Plugins.Clear();
        }

        public IEnumerable<AbstractParser> GetAllParsers()
        {
            return Plugins.SelectMany(p => p.GetParsers());
        }
        public IEnumerable<AbstractDevice> GetAllDevices()
        {
            return Plugins.SelectMany(p => p.GetDevices());
        }
        public void Dispose()
        {
            ClearPlugins();
        }
        
    }
}
#endif