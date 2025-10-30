using BlockIO.Arch;
using BlockIO.Arch.Windows;
using BlockIO.Generic;
using BlockIO.Interface;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace BlockIO.GPT
{
    /// <summary>
    /// Provides GPT-specific operations on a raw device stream.
    /// </summary>
    public class GPTDeviceStream : DeviceStream
    {
        public bool DryRun { get; set; } = false;
        

        public GPTDeviceStream(AbstractDevice device, FileAccess access, ulong startSector = 0)
            : base(device, startSector, access) { }

        /// <summary>
        /// Initializes a blank GPT layout on the device stream, overwriting any existing partition table.
        /// </summary>
        /// <returns>True if layout creation succeeded; false otherwise.</returns>
        public bool CreateEmptyGptLayout(out Exception? error)
        {
            if (Device.SectorCount < GptHeader.MinimumSectorCount)
                throw new InvalidOperationException("Device too small for GPT layout.");

            if (DryRun)
            {
                error = null;
                return true;
            }

            error = null;
            try
            {
                var header = GptHeader.CreateDefault(Device.SectorCount);
                var entries = GptEntryArray.CreateEmpty(128);

                GPTLayoutBuilder.WriteHeader(this, header, withBackupHeader: true);
                GPTLayoutBuilder.WriteEntryArray(this, entries, header.PartitionEntryLBA);
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }

            return true;
        }
        /// <summary>
        /// Attempts to read and validate the GPT header at LBA 1.
        /// </summary>
        /// <param name="header">The parsed GPT header if valid; otherwise null.</param>
        /// <param name="err">The exception encountered during read or validation.</param>
        /// <returns>True if a valid GPT header was found; false otherwise.</returns>
        public bool TryReadGptHeader(out GptHeader? header, ref Exception err)
        {
            header = null; 
            bool _ret = false;
            try
            {
                Seek(1 * Device.SectorSize, SeekOrigin.Begin);
                var headerBytes = new byte[Device.SectorSize];
                ReadExactly(headerBytes, 0, SectorSize);

                if (!GptHeader.IsValid(headerBytes))
                {
                    err = new ArgumentException("Invalid GPT header.");
                    _ret = false;
                }
                else
                {
                    header = GptHeader.FromBytes(headerBytes);
                    _ret = true;
                }
            }
            catch(Exception ex)
            {
                err = ex;
            }
            return _ret;
        }

        public bool IsValid()
        {
            Exception err = null!;
            GptHeader? header = null;
            return TryReadGptHeader(out header, ref err);
        }

        public void WriteSector(ulong lba, byte[] data)
        {
            if (data.Length != SectorSize)
                throw new ArgumentException("Data length does not match sector size.");
            if (DryRun)
                return;
            Seek((long)(lba * (ulong)SectorSize), SeekOrigin.Begin);
            Write(data, 0, data.Length);
        }

        public byte[] ReadSector(ulong lba)
        {
            var buffer = new byte[SectorSize];
            Seek((long)(lba * (ulong)SectorSize), SeekOrigin.Begin);
            ReadExactly(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}