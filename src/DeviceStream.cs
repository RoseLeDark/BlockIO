// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch.Windows;
using BlockIO.Interface;
using System.Collections.Concurrent;

namespace BlockIO
{
    /// <summary>
    /// Provides a raw stream over an entire device by wrapping it in a virtual partition.
    /// Inherits all block-level access behavior from <see cref="PartitionStream"/>.
    /// </summary>
    public class DeviceStream : PartitionStream
    {
        /// <summary>
        /// Initializes a new stream for accessing the device starting at a given sector.
        /// </summary>
        /// <param name="abstractDevice">The device to wrap and access.</param>
        /// <param name="startSector">The sector at which the stream begins (default is 0).</param>
        /// <param name="access">The desired access mode (read, write, or both).</param>
        /// <remarks>
        /// This stream is synthetic and does not reflect any physical partition table.
        /// Useful for forensic tools, recovery utilities, or device-wide analysis.
        /// </remarks>
        public DeviceStream(AbstractDevice abstractDevice, ulong startSector, FileAccess access)
            : base(new VirtualDevicePartition(abstractDevice, startSector), access)
        { }

        public async Task CreateSnapshotAsync(ulong startSector, ulong endSector, Stream destination, int bs = 4096)
        {
            ulong sectorCount = endSector - startSector + 1;
            ulong totalBytes = sectorCount * (ulong)SectorSize;
            byte[] buffer = new byte[bs];
            ulong bytesRemaining = totalBytes;

            Seek((long)(startSector * (ulong)SectorSize), SeekOrigin.Begin);

            while (bytesRemaining > 0)
            {
                int toRead = (int)Math.Min((ulong)buffer.Length, bytesRemaining);
                int bytesRead = await ReadAsync(buffer, 0, toRead);
                if (bytesRead == 0) break;

                await destination.WriteAsync(buffer, 0, bytesRead);
                bytesRemaining -= (ulong)bytesRead;
            }
        }
    }
}