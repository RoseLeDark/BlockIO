// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface.License;

namespace BlockIO.Interface
{
    public abstract class AbstractParser : IMetaInfo
    {
        /// <summary>
        /// Gets the device path used to identify the device instance.
        /// </summary>
        public abstract string Path { get;  }

        /// <summary>
        /// Gets the size, in bytes, of a single sector on the underlying storage device.
        /// </summary>
        public abstract uint SectorSize { get; }

        /// <summary>
        /// Gets the total number of partitions currently managed by the instance.
        /// </summary>
        public abstract int PartitionCount { get; internal set; }

        public abstract string Name { get; }
        public abstract VersionInfo Version { get; }
        public abstract string Author { get; }
        public abstract LicenseType License { get; }
        public abstract string Description { get; }

        /// <summary>
        /// When implemented in a derived class, gets the size of a single sector, in bytes, for the underlying storage
        /// medium.
        /// </summary>
        /// <returns>The size of a sector, in bytes.</returns>
        protected abstract uint GetSectorSize(string path);

        protected abstract bool CanParse(Stream stream, ref string errorString);

        public abstract List<AbstractPartition> parse(AbstractDevice device, uint sectorSize = 0);
    }
}
