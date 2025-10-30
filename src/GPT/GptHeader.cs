using System.Buffers.Binary;
using System.Text;

namespace BlockIO.GPT
{
    public class GptHeader
    {
        public const long Signature = 0x5452415020494645; // "EFI PART" in ASCII
        public const uint Revision = 0x00010000;
        public const int HeaderSize = 92;

        public const int MinimumSectorCount = 32;

        public ulong MyLBA { get; set; }
        public ulong AlternateLBA { get; set; }
        public ulong FirstUsableLBA { get; set; }
        public ulong LastUsableLBA { get; set; }
        public Guid DiskGuid { get; set; }
        public ulong PartitionEntryLBA { get; set; }
        public uint NumberOfEntries { get; set; } = 128;
        public uint EntrySize { get; set; } = 128;
        public uint HeaderCRC32 { get; set; } = 0;
        public uint PartitionArrayCRC32 { get; set; } = 0;

        public bool IsBackup => MyLBA > AlternateLBA;

        public static GptHeader CreateDefault(ulong sectorCount)
        {
            return new GptHeader
            {
                MyLBA = 1,
                AlternateLBA = sectorCount - 1,
                FirstUsableLBA = 34,
                LastUsableLBA = sectorCount - 34,
                DiskGuid = Guid.NewGuid(),
                PartitionEntryLBA = 2,
                NumberOfEntries = 128,
                EntrySize = 128
            };
        }
        public static bool TryFromBytes(byte[] buffer, out GptHeader? header)
        {
            header = null;
            if (!IsValid(buffer))
                return false;

            try
            {
                header = FromBytes(buffer);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static GptHeader FromBytes(byte[] buffer)
        {
            var span = buffer.AsSpan();
            /*if (BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(0, 8)) != Signature)
                throw new ArgumentException("Invalid GPT header signature.");*/
            // GPT Signatur prüfen

            /*var signature = System.Text.Encoding.ASCII.GetString(buffer, 0, 8);
            if (signature != "EFI PART")
                throw new ArgumentException("Invalid GPT header signature.");*/

            if(IsValid(buffer) == false)
                throw new ArgumentException("Invalid GPT header CRC.");

            return new GptHeader
            {
                MyLBA = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(24, 8)),
                AlternateLBA = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(32, 8)),
                FirstUsableLBA = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(40, 8)),
                LastUsableLBA = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(48, 8)),
                DiskGuid = new Guid(span.Slice(56, 16)),
                PartitionEntryLBA = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(72, 8)),
                NumberOfEntries = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(80, 4)),
                EntrySize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(84, 4)),
                HeaderCRC32 = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(16, 4)),
                PartitionArrayCRC32 = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(88, 4))
            };
        }

        public byte[] ToBytes()
        {
            var buffer = new byte[512];
            var span = buffer.AsSpan();

            Encoding.ASCII.GetBytes("EFI PART").CopyTo(span.Slice(0, 8));
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(8, 4), Revision);
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(12, 4), HeaderSize);
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(16, 4), HeaderCRC32); // optional: compute later
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(24, 8), MyLBA);
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(32, 8), AlternateLBA);
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(40, 8), FirstUsableLBA);
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(48, 8), LastUsableLBA);
            DiskGuid.ToByteArray().CopyTo(span.Slice(56, 16));
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(72, 8), PartitionEntryLBA);
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(80, 4), NumberOfEntries);
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(84, 4), EntrySize);
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(88, 4), PartitionArrayCRC32);

            return buffer;
        }
        public byte[] ToBackupBytes()
        {
            var backup = new GptHeader
            {
                MyLBA = this.AlternateLBA,
                AlternateLBA = this.MyLBA,
                FirstUsableLBA = this.FirstUsableLBA,
                LastUsableLBA = this.LastUsableLBA,
                DiskGuid = this.DiskGuid,
                PartitionEntryLBA = this.AlternateLBA - 32, // Backup entries before backup header
                NumberOfEntries = this.NumberOfEntries,
                EntrySize = this.EntrySize,
                HeaderCRC32 = 0, // Optional: recalculate
                PartitionArrayCRC32 = this.PartitionArrayCRC32
            };

            return backup.ToBytes();
        }
        public void ComputeHeaderCRC()
        {
            var buffer = ToBytes();
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(16, 4), 0);
            var crc32 = new System.IO.Hashing.Crc32();
            crc32.Append(buffer.AsSpan(0, HeaderSize));
            HeaderCRC32 = crc32.GetCurrentHashAsUInt32();
        }
        public static bool IsValid(byte[] buffer)
        {
            if (buffer.Length < HeaderSize) return false;

            var signature = Encoding.ASCII.GetString(buffer, 0, 8);
            if (signature != "EFI PART") return false;

            var span = buffer.AsSpan();
            var expectedCRC = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(16, 4));
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(16, 4), 0);

            var crc32 = new System.IO.Hashing.Crc32();
            crc32.Append(span.Slice(0, HeaderSize));
            return crc32.GetCurrentHashAsUInt32() == expectedCRC;
        }

        public GptHeader CloneAsBackup()
        {
            return new GptHeader
            {
                MyLBA = this.AlternateLBA,
                AlternateLBA = this.MyLBA,
                FirstUsableLBA = this.FirstUsableLBA,
                LastUsableLBA = this.LastUsableLBA,
                DiskGuid = this.DiskGuid,
                PartitionEntryLBA = this.AlternateLBA - 32,
                NumberOfEntries = this.NumberOfEntries,
                EntrySize = this.EntrySize,
                PartitionArrayCRC32 = this.PartitionArrayCRC32
            };
        }

        public bool ValidateCRC(byte[] buffer)
        {
            var span = buffer.AsSpan();
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(16, 4), 0);

            var crc32 = new System.IO.Hashing.Crc32();
            crc32.Append(span);
            return crc32.GetCurrentHashAsUInt32() == HeaderCRC32;
        }

    }
}
