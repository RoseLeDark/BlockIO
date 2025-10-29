// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.GPT;
using BlockIO.Interface.License;


namespace BlockIO.Interface
{
    /// <summary>
    /// Provides a scoped lock for an <see cref="AbstractPartition"/> instance.
    /// Automatically releases the lock when disposed.
    /// </summary>
    public class AbstractPartitionLockScope : IDisposable
    {
        private readonly AbstractPartition _partition;
        /// <summary>
        /// Acquires a lock on the specified partition.
        /// </summary>
        /// <param name="partition">The partition to lock.</param>
        public AbstractPartitionLockScope(AbstractPartition partition) => _partition = partition;
        /// <summary>
        /// Releases the lock on the partition.
        /// </summary>
        public void Dispose() => _partition.Unlock();
    }

    /// <summary>
    /// Represents a logical disk partition, providing properties and methods for managing partition metadata, access
    /// permissions, and cloning operations.
    /// </summary>
    /// <remarks>The AbstractPartition class encapsulates information about a disk partition, including its
    /// sector range, size, type, and access capabilities. It supports locking to prevent concurrent modifications and
    /// provides methods for cloning, resetting, and serializing partition data. Thread safety is managed via an
    /// internal synchronization object. Instances can be locked to restrict access, and the class implements
    /// IDisposable to release locks when no longer needed. Partition streams can be created for reading or writing,
    /// subject to access permissions and lock state.</remarks>
    public abstract class AbstractPartition : IDisposable, IMetaInfo, ICloneable
    {
        /// <summary>
        /// Internal synchronization object used for structural locking.
        /// </summary>
        private readonly object m_sync = new object();

        private AbstractDevice m_device;

        /// <summary>
        /// Gets the device path associated with this partition.
        /// If this is a clone, the path is inherited from the parent.
        /// </summary>
        public string DevicePath => Parent != null ? Parent.DevicePath : m_device.DevicePath;

        /// <summary>
        /// Gets or sets the unique partition ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the starting sector of the partition.
        /// </summary>
        public ulong StartSector { get; set; }

        /// <summary>
        /// Gets or sets the ending sector of the partition.
        /// </summary>
        public ulong EndSector { get; set; }

        /// <summary>
        /// Gets or sets the total number of sectors in the partition.
        /// </summary>
        public ulong SectorCount { get; set; }

        /// <summary>
        /// Gets or sets the sector size in bytes.
        /// </summary>
        public int SectorSize { get; set; }

        /// <summary>
        /// Indicates whether the partition is writable.
        /// </summary>
        public bool Writable { get; set; }

        /// <summary>
        /// Indicates whether the partition is readable.
        /// </summary>
        public bool Readable { get; set; }

        /// <summary>
        /// Indicates whether the partition is currently locked for structural operations.
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Gets or sets the partition type GUID.
        /// </summary>
        public Guid TypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique partition GUID.
        /// </summary>
        public Guid UniqueGuid { get; set; }

        /// <summary>
        /// Gets or sets the partition name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the device associated with this partition.
        /// If this is a clone, the device is inherited from the parent.
        /// </summary>
        public AbstractDevice Device => Parent != null ? Parent.m_device : m_device;

        /// <summary>
        /// Gets the parent partition if this is a clone; otherwise null.
        /// </summary>
        public AbstractPartition? Parent { get; protected set; }

        /// <inheritdoc/>
        public abstract VersionInfo Version { get; }

        /// <inheritdoc/>
        public abstract string Author { get; }

        /// <inheritdoc/>
        public abstract LicenseType License { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }

        /// <summary>
        /// Initializes a new partition with explicit sector boundaries and metadata.
        /// </summary>
        /// <param name="device">The underlying device associated with the partition.</param>
        /// <param name="Name">The name of the partition.</param>
        /// <param name="typeGuid">The partition type GUID (e.g. GPT type).</param>
        /// <param name="uniqueGuid">A unique GUID identifying this partition instance.</param>
        /// <param name="firstSector">The starting sector of the partition.</param>
        /// <param name="lastSector">The ending sector of the partition.</param>
        /// <param name="sectorSize">The size of each sector in bytes.</param>
        public AbstractPartition(AbstractDevice device, string Name, Guid typeGuid, Guid uniqueGuid, ulong firstSector, ulong lastSector, int sectorSize)
        {
            Writable = true;
            Readable = true;
            Locked = false;

            this.Name = Name;
            this.TypeGuid = typeGuid;
            this.UniqueGuid = uniqueGuid;

            this.StartSector = firstSector;
            this.EndSector = lastSector;
            this.SectorCount = (ulong)lastSector - (ulong)firstSector + 1;
            this.SectorSize = sectorSize;
            this.Id = (int)AbstractPartition.getNextID();
            Parent = null;
            m_device = device;
        }
        /// <summary>
        /// Initializes a virtual partition that spans the entire device.
        /// Used for synthetic or device-wide views.
        /// </summary>
        /// <param name="device">The underlying device to wrap.</param>
        /// <param name="Name">The name of the virtual partition.</param>
        internal AbstractPartition(AbstractDevice device, string Name)
        {
            Writable = true;
            Readable = true;
            Locked = false;

            this.Name = Name;
            this.TypeGuid = GPTTypeRegistry.TypeGuids[GPTType.VirtualPartitionFromDevice];
            this.UniqueGuid = Guid.NewGuid();

            this.StartSector = 0;
            this.EndSector = device.SectorCount - 1;
            this.SectorCount = device.SectorCount;
            this.SectorSize = device.SectorSize;
            this.Id = (int)AbstractPartition.getNextID();
            Parent = null;
            m_device = device;
        }


        /// <summary>
        /// Creates a structural clone of the partition with a restricted sector range.
        /// </summary>
        /// <param name="firstSector">The starting sector of the clone.</param>
        /// <param name="lastSector">The ending sector of the clone.</param>
        /// <returns>A new <see cref="AbstractPartition"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if partition is locked or already a clone.</exception>
        public PartitionStream CreateStream(FileAccess access)
        {


            if (Locked || !Readable && access == FileAccess.Read || !Writable && access == FileAccess.Write)
                throw new UnauthorizedAccessException("Partition is locked or access not allowed.");

            return new PartitionStream(this, access);
        }


        /// <inheritdoc/>
        public AbstractPartition? Clone(ulong firstSector, ulong lastSector)
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            if (this.Parent != null)
                throw new InvalidOperationException("Cloning of sub-partitions is not supported.");

            var clone = (AbstractPartition)this.MemberwiseClone();
            if (firstSector >= this.StartSector && lastSector <= this.EndSector - 1)
            {
                clone.StartSector = firstSector;
                clone.EndSector = lastSector;
            }

            clone.Parent = this;

            return clone;
        }
        /// <summary>
        /// Creates a clone of the partition with a new name.
        /// </summary>
        /// <param name="newName">The name to assign to the clone.</param>
        /// <returns>A new <see cref="AbstractPartition"/> instance.</returns>
        public object Clone()
        {
            if (this.Parent != null)
                throw new InvalidOperationException("Cloning of sub-partitions is not supported.");
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            var clone = (AbstractPartition)this.MemberwiseClone();
            OnCloned(clone);
            clone.Parent = this;
            return clone;
        }
        /// <summary>
        /// Called after cloning to allow derived classes to adjust internal state.
        /// </summary>
        /// <param name="clone">The cloned partition instance.</param>
        protected abstract void OnCloned(AbstractPartition clone);

        /// <summary>
        /// Creates a clone of the partition with a new name.
        /// Used to differentiate cloned partitions semantically (e.g. "Backup", "Slice").
        /// </summary>
        /// <param name="newName">The name to assign to the cloned partition.</param>
        /// <returns>A new <see cref="AbstractPartition"/> instance with the specified name.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the partition is locked or if cloning of sub-partitions is attempted.
        /// </exception>
        public AbstractPartition? Clone(string newName)
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");
            if (this.Parent != null)
                throw new InvalidOperationException("Cloning of sub-partitions is not supported.");

            var clone = (AbstractPartition)this.MemberwiseClone();
            clone.Name = newName;
            OnCloned(clone);
            clone.Parent = this;

            return clone;
        }

        /// <summary>
        /// Resets access flags and unlocks the partition.
        /// </summary>
        public void Reset()
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            Writable = true;
            Readable = true;
            Locked = false;
        }
        /// <summary>
        /// Locks the partition for exclusive structural access.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if already locked.</exception>
        public void Lock()
        {
            if (Locked) throw new InvalidOperationException("Partition is already locked.");
            Monitor.Enter(m_sync);
            Locked = true;
        }
        /// <summary>
        /// Unlocks the partition.
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
        /// <inheritdoc/>
        public void Dispose()
        {
            if (Locked)
            {
                Unlock();
            }
        }
        /// <summary>
        /// Acquires a scoped lock on the partition.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
        public IDisposable AcquireLock()
        {
            Lock();
            return new AbstractPartitionLockScope(this);
        }



        /// <inheritdoc/>
        public override string ToString()
        {
            var sizeMB = (double)SectorCount * SectorSize / 1024 / 1024;
            return $"Partition {Name} ({TypeGuid}), Sectors: {StartSector}-{EndSector} (Size: {sizeMB:F2} MB)";
        }

        private static long currentID = 0;

        /// <summary>
        /// Returns the next available partition ID.
        /// </summary>
        /// <returns>A unique long integer ID.</returns>
        private static long getNextID() => Interlocked.Increment(ref currentID);


    }
}