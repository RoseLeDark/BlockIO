using BlockIO.Interface;

namespace BlockIO.GPT
{
    public class GPTLayoutBuilder
    {
        public static void CreateEmptyLayout(GPTDeviceStream stream)
        {
            var device = stream.Device;

            var sectorSize = device.SectorSize;
            var sectorCount = device.SectorCount;

            var header = GptHeader.CreateDefault(sectorCount);
            var entries = GptEntryArray.CreateEmpty(128);
        }
        public static void WriteHeader(GPTDeviceStream stream, GptHeader header, bool withBackupHeader = false)
        {
            var sectorSize = stream.SectorSize;
            stream.Seek((long)(header.MyLBA * (ulong)sectorSize), SeekOrigin.Begin);
            stream.Write(header.ToBytes());

            if(withBackupHeader)
            {
                stream.Seek((long)(header.AlternateLBA * (ulong)sectorSize), SeekOrigin.Begin);
                stream.Write(header.ToBackupBytes());
            }
        }
        public static void WriteEntryArray(GPTDeviceStream stream, GptEntryArray entries, ulong startLBA)
        {
            var sectorSize = stream.SectorSize;
            stream.Seek((long)(startLBA * (ulong)sectorSize), SeekOrigin.Begin);
            stream.Write(entries.ToBytes());
        }
        public static void WriteBackupHeader(GPTDeviceStream stream, GptHeader header)
        {
            var sectorSize = stream.SectorSize;
            stream.Seek((long)(header.AlternateLBA * (ulong)sectorSize), SeekOrigin.Begin);
            stream.Write(header.ToBackupBytes());
        }

        
    }

}
