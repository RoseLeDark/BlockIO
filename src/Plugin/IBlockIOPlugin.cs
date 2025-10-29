// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
//
using BlockIO.Interface;

#if BLOCKIO_PLUGIN_SUPPORT

namespace BlockIO.Plugin
{
    /// <summary>
    /// Defines the contract for BlockIO plugins that provide device and parser implementations.
    /// Plugins must expose metadata and return discoverable components for registration.
    /// </summary>
    public interface IBlockIOPlugin
    {
        /// <summary>
        /// Gets the name of the plugin.
        /// Used for identification, logging, and manifest generation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a human-readable description of the plugin's purpose or capabilities.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Returns a collection of partition parsers provided by the plugin.
        /// </summary>
        /// <returns>An enumerable of <see cref="AbstractParser"/> instances.</returns>
        IEnumerable<AbstractParser> GetParsers();

        /// <summary>
        /// Returns a collection of device abstractions provided by the plugin.
        /// </summary>
        /// <returns>An enumerable of <see cref="AbstractDevice"/> instances.</returns>
        IEnumerable<AbstractDevice> GetDevices();
    }

}
#endif