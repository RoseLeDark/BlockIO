// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface;

namespace BlockIO
{
    public class DeviceStream : PartitionStream
    {
        public DeviceStream(AbstractDevice abstractDevice, FileAccess access)
            : base(new VirtualDevicePartition(abstractDevice), access)
        { }
    }
}