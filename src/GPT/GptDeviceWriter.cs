using BlockIO.Arch.Windows;
using BlockIO.GPT;
using System.Buffers.Binary;
using System.IO.Hashing;

namespace BlockIO.GPT
{
    /// <summary>
    /// Provides methods to write GPT structures to a raw device stream.
    /// </summary>
    public static class GptDeviceWriter
    {
        //TODO: ADD TO UTILS CLASS
        private static uint CreateCRC32(byte[] buffer)
        {
            var span = buffer.AsSpan();
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(16, 4), 0);

            var crc32 = new System.IO.Hashing.Crc32();
            crc32.Append(span);
            return crc32.GetCurrentHashAsUInt32();
        }

        /// <summary>
        /// Writes a default GPT header, empty partition entry array, and backup header to the device.
        /// </summary>
        /// <param name="stream">The GPT-capable device stream.</param>
        public static void WriteDefaultGpt(GPTDeviceStream stream)
        {
            var sectorSize = stream.SectorSize;
            var sectorCount = stream.Device.SectorCount;

            // 1. Protective MBR at sector 0
            var protectiveMbr = ProtectiveMBR.Create((ulong)sectorSize, sectorCount);
            stream.WriteSector(0, protectiveMbr);

            // 2. Primary GPT header
            var header = GptHeader.CreateDefault(sectorCount);
            var headerBytes = header.ToBytes();
            header.ComputeHeaderCRC(); // optional: recalculate CRC
            stream.WriteSector(header.MyLBA, headerBytes);

            // 3. Empty partition entry array (sectors 2–33)
            var entryArray = new byte[header.NumberOfEntries * header.EntrySize];

            header.PartitionArrayCRC32 = CreateCRC32(entryArray);
            var entrySectors = entryArray.Length / (int)sectorSize;

            for (int i = 0; i < entrySectors; i++)
            {
                var chunk = new byte[sectorSize];
                Array.Copy(entryArray, i * sectorSize, chunk, 0, sectorSize);
                stream.WriteSector(header.PartitionEntryLBA + (ulong)i, chunk);
            }

            // 4. Backup GPT header
            var backupBytes = header.ToBackupBytes();
            stream.WriteSector(header.AlternateLBA, backupBytes);

            // 5. Backup partition entry array
            ulong backupEntryLBA = header.AlternateLBA - (ulong)entrySectors;
            for (int i = 0; i < entrySectors; i++)
            {
                var chunk = new byte[sectorSize];
                Array.Copy(entryArray, i * sectorSize, chunk, 0, sectorSize);
                stream.WriteSector(backupEntryLBA + (ulong)i, chunk);
            }
        }
    }
}