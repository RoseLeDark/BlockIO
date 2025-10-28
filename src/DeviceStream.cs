// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface;

namespace BlockIO
{
    /// <summary>
    /// Provides a raw stream over an entire device by wrapping it in a virtual partition.
    /// Inherits all block-level access behavior from <see cref="PartitionStream"/>.
    /// </summary>
    public class DeviceStream : PartitionStream
    {
        /// <summary>
        /// Initializes a new stream for accessing the full device as a virtual partition.
        /// </summary>
        /// <param name="abstractDevice">The device to wrap and access.</param>
        /// <param name="access">The desired access mode (read, write, or both).</param>
        public DeviceStream(AbstractDevice abstractDevice, FileAccess access)
            : base(new VirtualDevicePartition(abstractDevice), access)
        { }
    }
}