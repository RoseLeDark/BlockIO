// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
using BlockIO.Interface;
using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32;
using Windows.Win32.System.Ioctl;

namespace BlockIO.Arch.Windows
{
    [SupportedOSPlatform("windows")]
    internal static class WindowsDriveInfo
    {
        /// <summary>
        /// Represents the physical geometry of a disk device, including cylinder, track, sector, and media type
        /// information.
        /// </summary>
        /// <remarks>This structure is typically used when interacting with low-level disk APIs to
        /// retrieve or specify disk layout details. The values correspond to hardware characteristics and may vary
        /// depending on the disk type and manufacturer. The <see cref="MediaType"/> field indicates the type of media,
        /// such as fixed or removable disk.</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct DISK_GEOMETRY
        {
            public long Cylinders;
            public MEDIA_TYPE MediaType;
            public uint TracksPerCylinder;
            public uint SectorsPerTrack;
            public uint BytesPerSector;
        }

        private static DISK_GEOMETRY? m_geometry;


        /// <summary>
        /// Represents the control code used to retrieve the geometry of a disk device in Windows I/O control
        /// operations.
        /// </summary>
        /// <remarks>This constant is typically used with device I/O control functions, such as
        /// DeviceIoControl, to obtain information about the physical characteristics of a disk, including its size,
        /// number of cylinders, tracks per cylinder, and sectors per track. The value corresponds to the
        /// IOCTL_DISK_GET_DRIVE_GEOMETRY code defined by the Windows API.</remarks>
        const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x70000;
        const uint IOCTL_DISK_GET_LENGTH_INFO = 0x7405C;

        /// <summary>
        /// Retrieves the sector size (in bytes) of the specified physical device.
        /// </summary>
        /// <param name="path">The device path (e.g., \\.\PhysicalDrive0).</param>
        /// <param name="errorstring">Returns error details if the query fails.</param>
        /// <returns>Sector size in bytes, or 0 if retrieval fails.</returns>
        /// <remarks>
        /// This method uses IOCTL_DISK_GET_DRIVE_GEOMETRY to query the device.
        /// The result reflects the physical block size used for LBA addressing.
        /// </remarks>
        internal static unsafe uint GetSectorSize(string path, ref string errorstring)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var handle = fs.SafeFileHandle.DangerousGetHandle();

            DISK_GEOMETRY geometry;
            uint bytesReturned;

            bool success = PInvoke.DeviceIoControl(
                fs.SafeFileHandle,
                IOCTL_DISK_GET_DRIVE_GEOMETRY,
                null,
                0,
                &geometry,
                (uint)Marshal.SizeOf<DISK_GEOMETRY>(),
                &bytesReturned,
                null );

            m_geometry = geometry;

            if (success)
            {
                errorstring = string.Empty;
                return (uint)geometry.BytesPerSector;
            }
            // If DeviceIoControl fails, throw an exception with the last Win32 error.
            errorstring = "The sector size cannot be retrieved from the device. Win32error code: " + Marshal.GetLastWin32Error().ToString();
            return 0;
        }

        internal static bool CanParse(string path, ref string errorString)
        {
            if (!path.StartsWith(@"\\.\PhysicalDrive"))
            {
                errorString = ("Ungültiger Pfad für ein physikalisches Laufwerk." + nameof(path));
                return false;
            }


            // Prüfen ob Windows das Laufwerk verwaltet
            if (IsManagedByWindows(path))
            {
                errorString = ("Das Laufwerk wird von Windows verwaltet. Bitte offline schalten.");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether the specified device is managed by Windows as a mounted disk with at least one logical
        /// partition.
        /// </summary>
        /// <remarks>This method is supported only on Windows platforms. The device is considered managed
        /// if it is recognized by Windows and has at least one associated logical partition. Use this method to verify
        /// whether a disk is accessible and mountable by Windows.</remarks>
        /// <param name="path">The device path to check, typically corresponding to a physical disk's DeviceID. Cannot be null.</param>
        /// <returns>true if the device is managed by Windows and has at least one logical partition; otherwise, false.</returns>
        internal static bool IsManagedByWindows(string path)
        {
            bool _ret = false;

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE MediaType != NULL");
            foreach (ManagementObject drive in searcher.Get())
            {
                string? deviceId = drive["DeviceID"]?.ToString();
                if (deviceId == path)
                {
                    // Wenn Partitionen vorhanden sind, ist das Laufwerk wahrscheinlich gemountet
                    using var partitionSearcher = new ManagementObjectSearcher(
                        $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{deviceId}'}} WHERE ResultClass=Win32_DiskPartition");

                    foreach (ManagementObject partition in partitionSearcher.Get())
                    {
                        using var logicalSearcher = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE ResultClass=Win32_LogicalDisk");

                        if (logicalSearcher.Get().Count > 0)
                        {
                            _ret = true;
                            break;
                        }
                    }
                }
            }

            return _ret;
        }

        public static DeviceType GetDeviceType(string path)
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject drive in searcher.Get())
            {
                string? deviceId = drive["DeviceID"]?.ToString();
                if (deviceId == path)
                {
                    string mediaType = drive["MediaType"]?.ToString() ?? "Unbekannt";
                    string interfaceType = drive["InterfaceType"]?.ToString() ?? "Unbekannt";

                    if (interfaceType == "USB") return DeviceType.USB;
                    if (mediaType.Contains("SSD", StringComparison.OrdinalIgnoreCase)) return DeviceType.SSD;
                    if (mediaType.Contains("NVMe", StringComparison.OrdinalIgnoreCase)) return DeviceType.NVMe;
                    if (mediaType.Contains("Fixed", StringComparison.OrdinalIgnoreCase)) return DeviceType.Fixed;

                }
            }

            if (File.Exists(path)) return DeviceType.File;

            return DeviceType.Unknowm;
        }

        /// <summary>
        /// Calculates the maximum Logical Block Address (LBA) of the device.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="errorString">Returns error details if the query fails.</param>
        /// <returns>Total number of addressable sectors minus one.</returns>
        /// <remarks>
        /// This method combines IOCTL_DISK_GET_LENGTH_INFO and IOCTL_DISK_GET_DRIVE_GEOMETRY.
        /// It does not interpret partition layout or filesystem.
        /// </remarks>
        internal static unsafe ulong GetMaxLBA(string devicePath, ref string errorString)
        {
            const uint IOCTL_DISK_GET_LENGTH_INFO = 0x7405C;
            const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x70000;

            using var fs = new FileStream(devicePath, FileMode.Open, FileAccess.Read);
            var handle = fs.SafeFileHandle.DangerousGetHandle();

            // Get disk size in bytes
            ulong diskSize = 0;
            uint bytesReturned;

            bool sizeSuccess = PInvoke.DeviceIoControl(
                fs.SafeFileHandle,
                IOCTL_DISK_GET_LENGTH_INFO,
                null,
                0,
                &diskSize,
                (uint)Marshal.SizeOf<ulong>(),
                &bytesReturned,
                null);

            if (!sizeSuccess)
            {
                errorString = ("Unable to retrieve disk size. Win32 error: " + Marshal.GetLastWin32Error());
                return 0; 
            }
            // Get sector size
            DISK_GEOMETRY geometry;
            bool geoSuccess = PInvoke.DeviceIoControl(
                fs.SafeFileHandle,
                IOCTL_DISK_GET_DRIVE_GEOMETRY,
                null,
                0,
                &geometry,
                (uint)Marshal.SizeOf<DISK_GEOMETRY>(),
                &bytesReturned,
                null);

            if (!geoSuccess || geometry.BytesPerSector == 0)
            {
                errorString = ("Unable to retrieve sector size. Win32 error: " + Marshal.GetLastWin32Error());
            }

            return (diskSize / geometry.BytesPerSector) - 1;
        }
    }
}
