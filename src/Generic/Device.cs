// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
using BlockIO.Arch.Windows;
using BlockIO.Interface;
using BlockIO.Interface.License;
using System.Runtime.InteropServices;

namespace BlockIO.Generic
{
    /// <summary>
    /// Represents a concrete storage device implementation using GPT parsing.
    /// Provides partition discovery, device classification, and lifecycle management.
    /// </summary>
    public class Device : AbstractDevice
    {
        /// <summary>
        /// Internal partition list managed by the device.
        /// </summary>
        private List<AbstractPartition> m_partitions => [];
        /// <summary>
        /// Stores the resolved structural classification of the device.
        /// </summary>
        protected DeviceType m_deviceType;

        /// <summary>
        /// Initializes a new device instance with the specified path and parser.
        /// Optionally triggers initialization during construction.
        /// </summary>
        /// <param name="devicePath">The device path used for access.</param>
        /// <param name="parser">The parser responsible for partition discovery.</param>
        /// <param name="bInitialisOnConstruct">If true, calls <see cref="Initialis"/> during construction.</param>
        public Device(string devicePath, AbstractParser parser, bool bInitialisOnConstruct = false)
            : base(devicePath, parser)
        {
            if(bInitialisOnConstruct) 
                Initialis();
        }

        /// <inheritdoc/>
        public override VersionInfo Version => VersionInfo.Default;
        /// <inheritdoc/>
        public override string Author => "BlockIO Team";
        /// <inheritdoc/>
        public override LicenseType License => LicenseType.EUPL12;
        /// <inheritdoc/>
        public override string Description => "GPT Device Handler";
        /// <inheritdoc/>
        public override DeviceType DeviceType => m_deviceType;

        /// <summary>
        /// Closes the device, clears partitions, and releases structural locks.
        /// </summary>
        public override void Close()
        {
            if(m_bLocked)
            {
                Unlock();
            }
            m_partitions.Clear();
            m_devicePath = String.Empty;
        }

        /// <summary>
        /// Returns all partitions currently managed by the device.
        /// </summary>
        /// <returns>A list of <see cref="AbstractPartition"/> instances.</returns>
        public override List<AbstractPartition> GetAllPartitions()
        {
            return m_partitions;
        }
        /// <summary>
        /// Resolves the structural classification of the device.
        /// </summary>
        /// <returns>A <see cref="DeviceType"/> value.</returns>
        protected virtual DeviceType GetDeviceType()
        {
            if(System.IO.File.Exists(m_devicePath))
                return DeviceType.File;
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsDriveInfo.GetDeviceType(m_devicePath);
            else
                return  DeviceType.Other;
        }

        /// <summary>
        /// Retrieves a partition by its unique GUID.
        /// </summary>
        /// <param name="guid">The partition GUID.</param>
        /// <returns>The matching partition, or null if not found.</returns>
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

        /// <summary>
        /// Retrieves a partition by its numeric ID.
        /// </summary>
        /// <param name="id">The partition ID.</param>
        /// <returns>The matching partition, or null if not found.</returns>
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

        /// <summary>
        /// Retrieves a partition by its name.
        /// </summary>
        /// <param name="name">The partition name.</param>
        /// <returns>The matching partition, or null if not found.</returns>
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

        /// <summary>
        /// Initializes the device by resolving sector size, sector count, and discovering partitions.
        /// </summary>
        public override void Initialis()
        {
            base.Initialis();

            m_partitions.Clear();
            m_partitions.AddRange(m_abstractParser.parse(this, (uint)m_sektorSize));
            m_deviceType = GetDeviceType();
        }

        /// <summary>
        /// Resets the device by closing and reinitializing it.
        /// </summary>
        public override void Reset()
        {
            Close();
            Initialis();
        }
    }
}
