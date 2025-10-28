// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
using BlockIO.Arch.Windows;
using BlockIO.Interface;
using BlockIO.Interface.License;
using System.Runtime.InteropServices;

namespace BlockIO
{
    public class Device : AbstractDevice
    {
        private List<AbstractPartition> m_partitions => [];
        protected DeviceType m_deviceType;

        public Device(string devicePath, AbstractParser parser, bool bInitialisOnConstruct = false)
            : base(devicePath, parser)
        {
            if(bInitialisOnConstruct) 
                Initialis();
        }


        public override VersionInfo Version => VersionInfo.Parse("1.0.0");

        public override string Author => "BlockIO Team";

        public override LicenseType License => LicenseType.EUPL12;

        public override string Description => "GPT Device Handler";

        public override DeviceType DeviceType => m_deviceType;

        public override void Close()
        {
            if(m_bLocked)
            {
                Unlock();
            }
            m_partitions.Clear();
            m_devicePath = String.Empty;
        }

        public override List<AbstractPartition> GetAllPartitions()
        {
            return m_partitions;
        }

        protected virtual DeviceType GetDeviceType()
        {
            if(System.IO.File.Exists(m_devicePath))
                return DeviceType.File;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsDriveInfo.GetDeviceType(m_devicePath);
            else
                return  DeviceType.Other;
        }

        public override AbstractPartition? GetPartitionByGuid(Guid guid)
        {
            foreach(var part in m_partitions)
            {
                if(part.UniqueGuid == guid)
                {
                    return part;
                }
            }
            return null;
        }

        public override AbstractPartition? GetPartitionById(int id)
        {
            foreach(var part in m_partitions)
            {
                if(part.Id == id)
                {
                    return part;
                }
            }
            return null;
        }

        public override AbstractPartition? GetPartitionByName(string name)
        {
            foreach (var part in m_partitions)
            {
                if (part.Name == Name)
                {
                    return part;
                }
            }
            return null;
        }

        public override void Initialis()
        {
            base.Initialis();

            m_partitions.Clear();
            m_partitions.AddRange(m_abstractParser.parse(this, (uint)m_sektorSize));
            m_deviceType = GetDeviceType();
        }

        public override void Reset()
        {
            Close();
            Initialis();
        }
    }
}
