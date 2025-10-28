// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch.Windows;
using BlockIO.Interface.License;
using System.Runtime.InteropServices;


namespace BlockIO.Interface
{
    public enum DeviceType
    {
        Unknowm = 0,
        SSD,
        NVMe,
        Fixed,
        HDD = Fixed,
        File,
        RAMDevice,
        Lo,
        USB,
        Other
    }
    public class AbstractDeviceLockScope : IDisposable
    {
        private readonly AbstractDevice _device;
        public AbstractDeviceLockScope(AbstractDevice device) => _device = device;
        public void Dispose() => _device.Unlock();
    }

    public abstract class AbstractDevice : IMetaInfo
    {
        /// <summary>
        /// Internal synchronization object used for thread-safe locking.
        /// </summary>
        private readonly object m_sync = new object();

        protected AbstractParser m_abstractParser;
        protected string m_devicePath;
        protected bool m_bLocked;
        protected int m_sektorSize;
        protected ulong m_sectorCount;

        public string Name { get; protected set; }
        public abstract VersionInfo Version { get; }
        public abstract string Author { get; }
        public abstract LicenseType License { get; }
        public abstract string Description { get; }

        public abstract DeviceType DeviceType { get; }

        public string DevicePath { get { return m_devicePath; } }

        public ulong SectorCount { get => m_sectorCount; internal set => m_sectorCount = value; }
        public int SectorSize { get => m_sektorSize; internal set => m_sektorSize = value; }

        public bool Locked { get => m_bLocked; protected set => m_bLocked = value; }

        public AbstractDevice(string devicePath, AbstractParser parser)
        {
            m_abstractParser = parser;
            m_devicePath = devicePath;
            m_bLocked = false;
            Name = string.Empty;
        }

        public virtual void Initialis()
        {
            SectorSize = (int)GetSectorSize(m_devicePath);
            SectorCount = GetMaxSectorCount(m_devicePath);
        }

        public abstract void Reset();

        public abstract void Close();

        public abstract AbstractPartition? GetPartitionById(int id);

        public abstract AbstractPartition? GetPartitionByName(string name);
        public abstract AbstractPartition? GetPartitionByGuid(Guid guid);
        public abstract List<AbstractPartition> GetAllPartitions();

        public void Lock()
        {
            if (Locked) throw new InvalidOperationException("Partition is already locked.");
            Monitor.Enter(m_sync);
            Locked = true;
        }
        public void Unlock()
        {
            if (!Locked) throw new InvalidOperationException("Partition is not locked.");
            try
            {
                Locked = false;
            }
            finally
            {
                Monitor.Exit(m_sync);
            }
        }

        public DeviceStream CreateDevicenStream(FileAccess access)
        {
            return new DeviceStream(this, access);
        }

        public PartitionStream CreatePartitionStream(AbstractPartition partition, FileAccess access)
        {
            if (partition == null)
                throw new ArgumentNullException(nameof(partition), "Partition cannot be null.");

            if (partition.Device == null || partition.Device.DevicePath != this.DevicePath)
                throw new ArgumentException("The partition does not belong to this device.", nameof(partition));

            return new PartitionStream(partition, access);
        }

        protected virtual uint GetSectorSize(string path)
        {
            uint _sektorSize = 512; // Standard Sektorgröße

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string errorString = string.Empty;
                _sektorSize = WindowsDriveInfo.GetSectorSize(path, ref errorString);
                if (_sektorSize == 0)
                    throw new InvalidOperationException(errorString);
            }
            return _sektorSize;
        }

        protected virtual ulong GetMaxSectorCount(string path)
        {
            ulong maxSectorCount = 0;
            string errorString = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                maxSectorCount = WindowsDriveInfo.GetMaxLBA(path, ref errorString) + 1; // LBA ist 0-basiert, daher +1 für die tatsächliche Anzahl
                if (maxSectorCount == 0)
                    throw new InvalidOperationException(errorString);
            }
            return maxSectorCount;

        }
    }
}
