// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;

namespace BlockIO.Arch.Windows
{
    /// <summary>
    /// Provides low-level read/write access to raw block devices on Windows.
    /// All operations are sector-aligned and operate on LBA-level offsets.
    /// </summary>
    [SupportedOSPlatform("windows")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen", Justification = "<Ausstehend>")]
    internal static class WindowsPartitionStream
    {
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 GENERIC_WRITE = 0x40000000;



        /// <summary>
        /// Reads raw sectors from the specified device using chunked overlapped reads.
        /// </summary>
        /// <param name="devicePath">The path to the physical device (e.g., \\.\PhysicalDrive0).</param>
        /// <param name="managedBuffer">The managed byte array to receive the read data.</param>
        /// <param name="offsetLba">The starting offset in Logical Block Addressing (LBA) units.</param>
        /// <param name="lenghtLBA">The number of sectors to read.</param>
        /// <param name="SectorSize">The size of each sector in bytes.</param>
        public static unsafe void ReadRaw(string devicePath, byte[] managedBuffer, long offsetLba, long lenghtLBA, long SectorSize)
        {
            long totalBytesToRead = lenghtLBA * SectorSize;
            long fileOffset = offsetLba * SectorSize;

            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_READ, FILE_SHARE_MODE.FILE_SHARE_READ,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);

            if (handle.IsInvalid)
            {
                throw new IOException("Failed to open device", Marshal.GetLastWin32Error());
            }

            PInvoke.SetFilePointerEx(handle, fileOffset, null, SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);

            int bufferOffset = 0;
            uint bytesRead = 0;

            while (totalBytesToRead > 0)
            {
                int chunkSize = (int)Math.Min(ArchPartitionStream.BlockSize, totalBytesToRead);
                Span<byte> lpBuffer = new Span<byte>(new byte[chunkSize]);
                try
                {
                    if (!PInvoke.ReadFile(handle, lpBuffer, &bytesRead, null))
                    {
                        throw new IOException("Failed to read from device", Marshal.GetLastWin32Error());
                    }

                    lpBuffer.CopyTo(managedBuffer.AsSpan(bufferOffset, (int)bytesRead));


                    bufferOffset += (int)bytesRead;
                    totalBytesToRead -= bytesRead;
                }
                finally
                {
                    lpBuffer.Clear();
                }
            }
        }

        /// <summary>
        /// Writes raw sectors to the specified device using chunked overlapped writes.
        /// </summary>
        /// <param name="devicePath">The path to the physical device (e.g., \\.\PhysicalDrive0).</param>
        /// <param name="managedBuffer">The managed byte array containing the data to write.</param>
        /// <param name="offsetLba">The starting offset in Logical Block Addressing (LBA) units.</param>
        /// <param name="lenghtLBA">The number of sectors to write.</param>
        /// <param name="SectorSize">The size of each sector in bytes.</param>
        public static unsafe void WriteRaw(string devicePath, byte[] managedBuffer, long offsetLba, long lenghtLBA, long SectorSize)
        {
            long totalBytesToWrite = lenghtLBA * SectorSize;
            long fileOffset = offsetLba * SectorSize;

            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);

            if (handle.IsInvalid)
            {
                throw new IOException("Failed to open device", Marshal.GetLastWin32Error());
            }

            PInvoke.SetFilePointerEx(handle, fileOffset, null, SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);

            int bufferOffset = 0;
            uint bytesWritten = 0;

            while (totalBytesToWrite > 0)
            {
                int chunkSize = (int)Math.Min(ArchPartitionStream.BlockSize, totalBytesToWrite);

                Span<byte> lpBuffer = new Span<byte>(new byte[chunkSize]);
                try
                {
                    managedBuffer.AsSpan(bufferOffset, chunkSize).CopyTo(lpBuffer);
                    if (!PInvoke.WriteFile(handle, lpBuffer, &bytesWritten, null))
                    {
                        throw new IOException("Failed to write to device", Marshal.GetLastWin32Error());
                    }
                    bufferOffset += (int)bytesWritten;
                    totalBytesToWrite -= bytesWritten;
                }
                finally
                {
                    lpBuffer.Clear();
                }
            }
        }
        /// <summary>
        /// Checks whether the specified device is accessible for raw reading.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <returns>True if the device can be opened for reading; otherwise, false.</returns>
        public static bool IsRawDeviceAccessible(string devicePath)
        {
            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_READ, FILE_SHARE_MODE.FILE_SHARE_READ,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);
            return !handle.IsInvalid;
        }
        /// <summary>
        /// Checks whether the specified device is writable using raw access.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <returns>True if the device can be opened for writing; otherwise, false.</returns>
        public static bool IsRawDeviceWritable(string devicePath)
        {
            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);
            return !handle.IsInvalid;
        }
        /// <summary>
        /// Flushes all buffered data to the specified device.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <exception cref="IOException">Thrown if the device cannot be opened or flushed.</exception>
        public static void FlushDevice(string devicePath)
        {
            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);
            if (handle.IsInvalid)
            {
                throw new IOException("Failed to open device", Marshal.GetLastWin32Error());
            }
            if (!PInvoke.FlushFileBuffers(handle))
            {
                throw new IOException("Failed to flush device buffers", Marshal.GetLastWin32Error());
            }

        }
        /// <summary>
        /// Attempts to discard the device's internal cache by flushing buffers.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <remarks>
        /// Windows does not support explicit cache discard like Linux's BLKDISCARD.
        /// This method uses FlushFileBuffers as the closest available operation.
        /// </remarks>
        /// <exception cref="IOException">Thrown if the device cannot be opened or flushed.</exception>
        public static void DiscardDeviceCache(string devicePath)
        {
            using var handle = PInvoke.CreateFile(devicePath,
                GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);
            if (handle.IsInvalid)
            {
                throw new IOException("Failed to open device", Marshal.GetLastWin32Error());
            }
            // Windows does not provide a direct way to discard device cache like Linux's BLKDISCARD.
            // Flushing the buffers is the closest equivalent.
            if (!PInvoke.FlushFileBuffers(handle))
            {
                throw new IOException("Failed to flush device buffers", Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Flushes the device's write cache to ensure data integrity.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <exception cref="IOException">Thrown if the device cannot be opened or flushed.</exception>
        public static void FlushDeviceCache(string devicePath)
        {
            using SafeFileHandle handle = PInvoke.CreateFile(devicePath,
                GENERIC_WRITE, FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null, FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                0, null);
            if (handle.IsInvalid)
            {
                throw new IOException("Failed to open device", Marshal.GetLastWin32Error());
            }
            if (!PInvoke.FlushFileBuffers(handle))
            {
                throw new IOException("Failed to flush device cache", Marshal.GetLastWin32Error());
            }
        }
    }
}