namespace BlockIO.GPT
{
    public class GPTLayoutBuilder
    {
        public static void CreateEmptyLayout(GPTDevice device)
        {
            using var stream = device.CreateDevicenStream(FileAccess.Write);

            var sectorSize = device.SectorSize;
            var sectorCount = device.SectorCount;

            var header = GptHeader.CreateDefault(sectorCount);
            var entries = GptEntryArray.CreateEmpty(128);

            // Write primary GPT header at LBA 1
            stream.Seek(sectorSize * 1, SeekOrigin.Begin);
            stream.Write(header.ToBytes());

            // Write partition entries at LBA 2
            stream.Seek(sectorSize * 2, SeekOrigin.Begin);
            stream.Write(entries.ToBytes());

            // Write backup GPT header at last LBA
            stream.Seek((((long)sectorCount - 1) * sectorSize), SeekOrigin.Begin);
            stream.Write(header.ToBackupBytes());
        }
    }

}
