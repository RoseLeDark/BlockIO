// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface.License;

namespace BlockIO.Interface
{
    /// <summary>
    /// Defines a base class for partition parsers that operate on raw device streams.
    /// Provides metadata, parsing capabilities, and sector size resolution.
    /// </summary>
    public abstract class AbstractParser : IMetaInfo
    {
        /// <summary>
        /// Gets the device path used to identify the device instance.
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// Gets the size, in bytes, of a single sector on the underlying storage device.
        /// </summary>
        public abstract uint SectorSize { get; }

        /// <summary>
        /// Gets the total number of partitions currently managed by the instance.
        /// </summary>
        public abstract int PartitionCount { get; internal set; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract VersionInfo Version { get; }

        /// <inheritdoc/>
        public abstract string Author { get; }

        /// <inheritdoc/>
        public abstract LicenseType License { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }


        /// <summary>
        /// When implemented in a derived class, gets the size of a single sector, in bytes, for the underlying storage
        /// medium.
        /// </summary>
        /// <returns>The size of a sector, in bytes.</returns>
        protected abstract uint GetSectorSize(string path);

        /// <summary>
        /// Determines whether the given stream can be parsed by this parser.
        /// </summary>
        /// <param name="stream">The input stream to evaluate.</param>
        /// <param name="errorString">A reference string to receive error details if parsing fails.</param>
        /// <returns>True if the stream is compatible; otherwise, false.</returns>
        protected abstract bool CanParse(Stream stream, ref string errorString);

        /// <summary>
        /// Parses the device and returns a list of discovered partitions.
        /// </summary>
        /// <param name="device">The device to parse.</param>
        /// <param name="sectorSize">Optional override for sector size. If 0, the parser resolves it automatically.</param>
        /// <returns>A list of <see cref="AbstractPartition"/> instances representing discovered partitions.</returns>
        public abstract List<AbstractPartition> parse(AbstractDevice device, uint sectorSize = 0);
    }
}
