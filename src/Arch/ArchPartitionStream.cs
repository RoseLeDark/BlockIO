// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch.Windows;
using System.Runtime.InteropServices;


namespace BlockIO.Arch
{
    /// <summary>
    /// Provides platform-aware access to raw partition streams, including read/write operations and block alignment.
    /// Delegates OS-specific logic to architecture modules (e.g., WindowsPartitionStream).
    /// </summary>
    internal class ArchPartitionStream
    {
        private static int m_iBlockSize = 1024 * 1024; // 1 MB block size

        /// <summary>
        /// Sets the global block size used for chunked I/O operations.
        /// Must be a positive power of two and ≥ 64 bytes.
        /// </summary>
        /// <param name="blockSize">The desired block size in bytes.</param>
        public static void SetBlockSize(int blockSize)
        {
            if (blockSize <= 64 || (blockSize & (blockSize - 1)) != 0)
            {
                throw new ArgumentException("Block size must be a positive power of two.");
            }
            m_iBlockSize = blockSize;
        }
        /// <summary>
        /// Gets the current block size used for I/O chunking.
        /// </summary>
        /// <returns>The block size in bytes.</returns>
        public static int GetCurrentBlockSize()
        {
            return m_iBlockSize;
        }

        /// <summary>
        /// Resets the block size to the default value (1 MB).
        /// </summary>
        public static void ResetBlockSize()
        {
            m_iBlockSize = 1024 * 1024; // Reset to default 1 MB block size
        }

        /// <summary>
        /// Gets or sets the block size with validation.
        /// </summary>
        public static int BlockSize { get => m_iBlockSize; set => SetBlockSize(value); }

        /// <summary>
        /// Aligns a given size to the next multiple of the current block size.
        /// </summary>
        /// <param name="size">The size in bytes to align.</param>
        /// <returns>The aligned size in bytes.</returns>
        public static long AlignToBlockSize(long size)
        {
            if (size % m_iBlockSize == 0)
            {
                return size;
            }
            return ((size / m_iBlockSize) + 1) * m_iBlockSize;
        }

        /// <summary>
        /// Returns the block-aligned base offset for a given byte offset.
        /// </summary>
        /// <param name="offset">The byte offset to align.</param>
        /// <returns>The aligned base offset.</returns>
        public static long GetBlockAlignedOffset(long offset)
        {
            return (offset / m_iBlockSize) * m_iBlockSize;
        }

        /// <summary>
        /// Returns the offset within a block for a given byte offset.
        /// </summary>
        /// <param name="offset">The byte offset to evaluate.</param>
        /// <returns>The offset within the block.</returns>
        public static int GetBlockAlignedOffsetInBlock(long offset)
        {
            return (int)(offset % m_iBlockSize);
        }

        /// <summary>
        /// Gets the current block size (alias for GetCurrentBlockSize).
        /// </summary>
        /// <returns>The block size in bytes.</returns>
        public static int GetBlockSize()
        {
            return m_iBlockSize;
        }


        /// <summary>
        /// Reads a single byte from a device at the specified byte offset.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to read from.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        /// <returns>The byte value read from the device.</returns>
        public static byte ReadByte(string devicePath, long offset, long SectorSize)
        {
            byte[] buffer = new byte[1];
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.ReadRaw(devicePath, buffer, offset / SectorSize, 1, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("WriteByte operation is not implemented for this OS.");
            }

            return buffer[0];
        }

        /// <summary>
        /// Writes a single byte to a device at the specified byte offset.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to write to.</param>
        /// <param name="value">The byte value to write.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        public static void WriteByte(string devicePath, long offset, byte value, long SectorSize)
        {
            byte[] buffer = new byte[1] { value };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.WriteRaw(devicePath, buffer, offset / SectorSize, 1, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("WriteByte operation is not implemented for this OS.");
            }
        }

        /// <summary>
        /// Writes a single byte to a device at the specified byte offset.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to write to.</param>
        /// <param name="value">The byte value to write.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        public static void ReadBytes(string devicePath, long offset, byte[] buffer, int count, long SectorSize)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.ReadRaw(devicePath, buffer, offset / SectorSize, count / SectorSize, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("ReadBytes operation is not implemented for this OS.");
            }
        }
        /// <summary>
        /// Writes a single byte to a device at the specified byte offset.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to write to.</param>
        /// <param name="value">The byte value to write.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        public static void WriteBytes(string devicePath, long offset, byte[] buffer, int count, long SectorSize)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.WriteRaw(devicePath, buffer, offset / SectorSize, count / SectorSize, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("WriteBytes operation is not implemented for this OS.");
            }
        }

        /// <summary>
        /// Reads a block-aligned byte range from the device into a span buffer.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to start reading from.</param>
        /// <param name="buffer">The span buffer to store the read data.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        public static void ReadBytes(string devicePath, long offset, Span<byte> buffer, long SectorSize)
        {
            byte[] tempBuffer = new byte[buffer.Length];
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.ReadRaw(devicePath, tempBuffer, offset / SectorSize, buffer.Length / SectorSize, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("ReadBytes operation is not implemented for this OS.");
            }
            tempBuffer.AsSpan(0, buffer.Length).CopyTo(buffer);
        }

        /// <summary>
        /// Writes a block-aligned byte range to the device from a span buffer.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <param name="offset">The byte offset to start writing to.</param>
        /// <param name="buffer">The span buffer containing the data to write.</param>
        /// <param name="SectorSize">The sector size in bytes.</param>
        public static void WriteBytes(string devicePath, long offset, ReadOnlySpan<byte> buffer, long SectorSize)
        {
            byte[] tempBuffer = new byte[buffer.Length];
            buffer.Slice(0, buffer.Length).CopyTo(tempBuffer);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.WriteRaw(devicePath, tempBuffer, offset / SectorSize, buffer.Length / SectorSize, SectorSize);
            }
            else
            {
                throw new PlatformNotSupportedException("WriteRaw operation is not implemented for this OS.");
            }
        }


        /// <summary>
        /// Checks whether the specified device is accessible for raw reading.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <returns>True if the device can be opened for reading; otherwise, false.</returns>
        public static bool IsRawDeviceAccessible(string devicePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsPartitionStream.IsRawDeviceAccessible(devicePath);
            }
            else
            {
                throw new PlatformNotSupportedException("IsRawDeviceAccessible operation is not implemented for this OS.");
            }
        }
        /// <summary>
        /// Checks whether the specified device is writable using raw access.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        /// <returns>True if the device can be opened for writing; otherwise, false.</returns>
        public static bool IsRawDeviceWritable(string devicePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsPartitionStream.IsRawDeviceWritable(devicePath);
            }
            else
            {
                throw new PlatformNotSupportedException("IsRawDeviceWritable operation is not implemented for this OS.");
            }
        }
        /// <summary>
        /// Flushes all buffered data to the specified device.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        public static void FlushDevice(string devicePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.FlushDevice(devicePath);
            }
            else
            {
                throw new PlatformNotSupportedException("FlushDevice operation is not implemented for this OS.");
            }

        }
        /// <summary>
        /// Attempts to discard the device's internal cache by flushing buffers.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        public static void DiscardDeviceCache(string devicePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.DiscardDeviceCache(devicePath);
            }
            else
            {
                throw new PlatformNotSupportedException("DiscardDeviceCache operation is not implemented for this OS.");
            }
        }

        /// <summary>
        /// Flushes the device's write cache to ensure data integrity.
        /// </summary>
        /// <param name="devicePath">The path to the physical device.</param>
        public static void FlushDeviceCache(string devicePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsPartitionStream.FlushDeviceCache(devicePath);
            }
            else
            {
                throw new PlatformNotSupportedException("FlushDeviceCache operation is not implemented for this OS.");
            }
        }
    }
}
