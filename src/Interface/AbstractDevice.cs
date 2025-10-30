// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch;
using BlockIO.Interface.License;


namespace BlockIO.Interface
{
    /// <summary>
    /// Represents the structural classification of a storage device.
    /// Used to determine how the device should be handled or instantiated within the system.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// The device type could not be determined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A solid-state drive (SSD) device.
        /// </summary>
        SSD,

        /// <summary>
        /// A Non-Volatile Memory Express (NVMe) device.
        /// </summary>
        NVMe,

        /// <summary>
        /// A fixed hard disk drive (HDD) or internal storage.
        /// </summary>
        Fixed,

        /// <summary>
        /// Alias for <see cref="Fixed"/> — represents a traditional hard disk drive.
        /// </summary>
        HDD = Fixed,

        /// <summary>
        /// A file-based device, such as a disk image (e.g., VHD, VHDX).
        /// </summary>
        File,

        /// <summary>
        /// A RAM-backed virtual device (e.g., RAM disk).
        /// </summary>
        RAMDevice,

        /// <summary>
        /// A loopback or virtual device (e.g., mounted via software).
        /// </summary>
        Removable,

        /// <summary>
        /// A USB-connected removable device.
        /// </summary>
        USB,

        /// <summary>
        /// A Virtual Hard Disk (VHD) device.
        /// </summary>
        VHD,

        /// <summary>
        /// Any other device type not explicitly classified.
        /// </summary>
        Other
    }

    /// <summary>
    /// Provides a scoped lock for an <see cref="AbstractDevice"/> instance.
    /// Automatically releases the lock when disposed.
    /// </summary>
    public class AbstractDeviceLockScope : IDisposable
    {
        private readonly AbstractDevice _device;

        /// <summary>
        /// Acquires a lock on the specified device.
        /// </summary>
        /// <param name="device">The device to lock.</param>
        public AbstractDeviceLockScope(AbstractDevice device) => _device = device;

        /// <summary>
        /// Releases the lock on the device.
        /// </summary>
        public void Dispose() => _device.Unlock();
    }

    /// <summary>
    /// Represents a structural abstraction of a storage device.
    /// Provides metadata, locking, partition access, and stream creation.
    /// </summary>
    public abstract class AbstractDevice : IMetaInfo, IDisposable
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

        /// <summary>
        /// Gets the display name of the device.
        /// </summary>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public abstract VersionInfo Version { get; }

        /// <inheritdoc/>
        public abstract string Author { get; }

        /// <inheritdoc/>
        public abstract LicenseType License { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }

        /// <summary>
        /// Gets the structural classification of the device.
        /// </summary>
        public abstract DeviceType DeviceType { get; }

        /// <summary>
        /// Gets the device path used for access and identification.
        /// </summary>
        public string DevicePath => m_devicePath;

        /// <summary>
        /// Gets or sets the total number of sectors on the device.
        /// </summary>
        public ulong SectorCount
        {
            get => m_sectorCount;
            internal set => m_sectorCount = value;
        }

        /// <summary>
        /// Gets or sets the size of a single sector in bytes.
        /// </summary>
        public int SectorSize
        {
            get => m_sektorSize;
            internal set => m_sektorSize = value;
        }

        /// <summary>
        /// Indicates whether the device is currently locked for structural operations.
        /// </summary>
        public bool Locked
        {
            get => m_bLocked;
            protected set => m_bLocked = value;
        }



        /// <summary>
        /// Initializes a new device abstraction with the specified path and parser.
        /// </summary>
        /// <param name="devicePath">The device path used for access.</param>
        /// <param name="parser">The parser associated with this device.</param>
        public AbstractDevice(string devicePath, AbstractParser parser)
        {
            m_abstractParser = parser;
            m_devicePath = devicePath;
            m_bLocked = false;
            Name = string.Empty;
        }

        /// <summary>
        /// Initializes the device by resolving sector size and total sector count.
        /// </summary>
        public virtual void Initialis()
        {
            SectorSize = (int)GetSectorSize(m_devicePath);
            SectorCount = GetMaxSectorCount(m_devicePath);
        }

        /// <summary>
        /// Resets the device state and releases resources.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Closes the device and finalizes access.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Retrieves a partition by its numeric ID.
        /// </summary>
        /// <param name="id">The partition ID.</param>
        /// <returns>The matching partition, or null if not found.</returns>
        public abstract AbstractPartition? GetPartitionById(int id);

        /// <summary>
        /// Retrieves a partition by its name.
        /// </summary>
        /// <param name="name">The partition name.</param>
        /// <returns>The matching partition, or null if not found.</returns>
        public abstract AbstractPartition? GetPartitionByName(string name);
        /// <summary>
        /// Retrieves a partition by its unique GUID.
        /// </summary>
        /// <param name="guid">The partition GUID.</param>
        /// <returns>The matching partition, or null if not found.</returns>
        public abstract AbstractPartition? GetPartitionByGuid(Guid guid);
        /// <summary>
        /// Returns all partitions associated with this device.
        /// </summary>
        /// <returns>A list of <see cref="AbstractPartition"/> instances.</returns>
        public abstract List<AbstractPartition> GetAllPartitions();

        /// <summary>
        /// Locks the device for exclusive structural access.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if already locked.</exception>
        public void Lock()
        {
            if (Locked) throw new InvalidOperationException("Partition is already locked.");
            Monitor.Enter(m_sync);
            Locked = true;
        }
        /// <summary>
        /// Unlocks the device.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not currently locked.</exception>
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

        /// <summary>
        /// Creates a raw device stream for reading or writing.
        /// </summary>
        /// <param name="access">The desired access mode.</param>
        /// <returns>A <see cref="DeviceStream"/> instance.</returns>
        public DeviceStream CreateDevicenStream(FileAccess access)
        {
            return new DeviceStream(this, 0, access);
        }

        /// <summary>
        /// Creates a partition stream for reading or writing a specific partition.
        /// </summary>
        /// <param name="partition">The partition to access.</param>
        /// <param name="access">The desired access mode.</param>
        /// <returns>A <see cref="PartitionStream"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if partition is null.</exception>
        /// <exception cref="ArgumentException">Thrown if partition does not belong to this device.</exception>
        public PartitionStream CreatePartitionStream(AbstractPartition partition, FileAccess access)
        {
            if (partition == null)
                throw new ArgumentNullException(nameof(partition), "Partition cannot be null.");

            if (partition.Device == null || partition.Device.DevicePath != this.DevicePath)
                throw new ArgumentException("The partition does not belong to this device.", nameof(partition));

            return new PartitionStream(partition, access);
        }

        /// <summary>
        /// Resolves the sector size for the device path.
        /// </summary>
        /// <param name="path">The device path to inspect.</param>
        /// <returns>The sector size in bytes.</returns>
        /// <exception cref="InvalidOperationException">Thrown if resolution fails.</exception>
        protected virtual uint GetSectorSize(string path)
        {
            uint _sektorSize = 512; // Standard Sektorgröße

            string errorString = string.Empty;
            _sektorSize = ArchDriveInfo.GetSectorSize(path, ref errorString);
            if (_sektorSize == 0)
                throw new InvalidOperationException(errorString);

            return _sektorSize;
        }

        /// <summary>
        /// Resolves the maximum sector count for the device path.
        /// </summary>
        /// <param name="path">The device path to inspect.</param>
        /// <returns>The total number of sectors.</returns>
        /// <exception cref="InvalidOperationException">Thrown if resolution fails.</exception>
        protected virtual ulong GetMaxSectorCount(string path)
        {
            ulong maxSectorCount = 0;
            string errorString = string.Empty;

            maxSectorCount = ArchDriveInfo.GetMaxLBA(path, ref errorString) + 1; // LBA ist 0-basiert, daher +1 für die tatsächliche Anzahl
            if (maxSectorCount == 0)
                throw new InvalidOperationException(errorString);
            return maxSectorCount;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Locked)
            {
                Unlock();
            }
        }
        /// <summary>
        /// Acquires a scoped lock on the device.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
        public IDisposable AcquireLock()
        {
            Lock();
            return new AbstractDeviceLockScope(this);
        }
    }
}
