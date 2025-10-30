using System;
using System.Buffers.Binary;

namespace BlockIO.GPT
{
    public static class ProtectiveMBR
    {
        public static byte[] Create(ulong sectorSize, ulong totalSectors)
        {
            var buffer = new byte[sectorSize];
            var span = buffer.AsSpan();

            // Boot code: optional, leave zeroed
            // Partition entry at offset 446
            span[446 + 0] = 0x00; // Boot indicator
            span[446 + 1] = 0xFF; // CHS start (dummy)
            span[446 + 2] = 0xFF;
            span[446 + 3] = 0xFF;
            span[446 + 4] = 0xEE; // Partition type: GPT protective
            span[446 + 5] = 0xFF; // CHS end (dummy)
            span[446 + 6] = 0xFF;
            span[446 + 7] = 0xFF;

            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(446 + 8, 4), 1); // LBA start
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(446 + 12, 4), (uint)Math.Min(totalSectors - 1, uint.MaxValue)); // LBA length

            // MBR signature
            span[510] = 0x55;
            span[511] = 0xAA;

            return buffer;
        }
    }
}
