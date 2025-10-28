// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.GPT;
using BlockIO.Interface.License;
using System.Reflection.Metadata;
using System.Xml;

namespace BlockIO.Interface
{
    
    public  class AbstractPartitionLockScope : IDisposable
    {
        private readonly AbstractPartition _partition;
        public AbstractPartitionLockScope(AbstractPartition partition) => _partition = partition;
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
    public abstract class AbstractPartition :  IDisposable, IMetaInfo, ICloneable
    {
        

        /// <summary>
        /// Internal synchronization object used for thread-safe locking.
        /// </summary>
        private readonly object m_sync = new object();

        private AbstractDevice m_device;

        public string DevicePath { get { return Parent != null ? Parent.DevicePath : m_device.DevicePath; } }

        public int Id { get; set; }

        public ulong StartSector { get; set; }
        public ulong EndSector { get; set; }
        public ulong SectorCount { get; set; }
        public int SectorSize { get; set; }

        public bool Writable { get; set; }
        public bool Readable { get; set; }
        public bool Locked { get; set; }

        public Guid TypeGuid { get; set; }
        public Guid UniqueGuid { get; set; }
        public string Name { get; set; }

        public AbstractDevice Device { get { return Parent != null ? Parent.m_device : m_device; } private set { m_device = value; } }

        public AbstractPartition? Parent { get; protected set; }
        public abstract VersionInfo Version { get; }
        public abstract string Author { get; }
        public abstract LicenseType License { get; }
        public abstract string Description { get; }

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

        internal AbstractPartition(AbstractDevice device, string Name)
        {
            Writable = true;
            Readable = true;
            Locked = false;

            this.Name = Name;
            this.TypeGuid = GPTTypeRegistry.TypeGuids[GPTType.VirtualPartitionFromDevice];
            this.UniqueGuid = Guid.NewGuid();

            this.StartSector = 0;
            this.EndSector = device.SectorCount;
            this.SectorCount = device.SectorCount + 1;
            this.SectorSize = device.SectorSize;
            this.Id = (int)AbstractPartition.getNextID();
            Parent = null;
            m_device = device;
        }


        public PartitionStream CreateStream(FileAccess access)
        {

            
            if (Locked || !Readable && access == FileAccess.Read || !Writable && access == FileAccess.Write)
                throw new UnauthorizedAccessException("Partition is locked or access not allowed.");

            return new PartitionStream(this, access);
        }


        public AbstractPartition? Clone(ulong firstSector, ulong lastSectort)
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            var clone = (AbstractPartition)this.MemberwiseClone();
            if (firstSector >= this.StartSector && lastSectort <= this.EndSector - 1)
            {
                clone.StartSector = firstSector;
                clone.EndSector = lastSectort;
            }

            return clone;
        }
        public object Clone()
        {
            var clone = (AbstractPartition)this.MemberwiseClone();
            OnCloned(clone);
            return clone;
        }
        protected abstract void OnCloned(AbstractPartition clone);
        public object Clone(string newName)
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            var clone = (AbstractPartition)this.MemberwiseClone();
            clone.Name = newName;
            OnCloned(clone);

            return clone;
        }

        public void Reset()
        {
            if (Locked) throw new InvalidOperationException("Partition is locked or access not allowed.");

            Writable = true;
            Readable = true;
            Locked = false;
        }
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
        public void Dispose()
        {
            if (Locked)
            {
                Unlock();
            }
        }
        public IDisposable AcquireLock()
        {
            Lock();
            return new AbstractPartitionLockScope(this);
        }



        public override string ToString()
        {
            return $"Partition {Name} ({TypeGuid}), Sectors: {StartSector}-{EndSector} (Size: {(ulong)SectorCount * (ulong)SectorSize / 1024 / 1024} MB)";
        }

        private static long currentID = 0;

        /// <summary>
        /// Returns the next available partition ID.
        /// </summary>
        /// <returns>Unique long integer ID.</returns>
        private static long getNextID()
        {
            return currentID++;
        }

        
    }
}