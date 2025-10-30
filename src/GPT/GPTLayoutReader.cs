using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    public static class GPTLayoutReader
    {
        public static GptHeader ReadGPTHeader(GPTDeviceStream stream, bool backup = false)
        {
            var sectorSize = stream.SectorSize;
            ulong lba = backup ? (stream.Device.SectorCount - 1) : 1;
            stream.Seek((long)(lba * (ulong)sectorSize), SeekOrigin.Begin);

            byte[] buffer = new byte[sectorSize];
            stream.ReadExactly(buffer, 0, sectorSize);
            return GptHeader.FromBytes(buffer);
        }

        public static GptEntryArray ReadEntryArray(GPTDeviceStream stream, GptHeader header)
        {
            var sectorSize = stream.SectorSize;
            ulong entryArraySize = header.NumberOfEntries * header.EntrySize;
            ulong entrySectors = (entryArraySize + (ulong)(sectorSize - 1)) / (ulong)sectorSize;
            stream.Seek((long)(header.PartitionEntryLBA * (ulong)sectorSize), SeekOrigin.Begin);
            byte[] buffer = new byte[entryArraySize];
            stream.ReadExactly(buffer, 0, (int)entryArraySize);
            return GptEntryArray.FromBytes(buffer, header.NumberOfEntries, header.EntrySize);
        }
    }
}
