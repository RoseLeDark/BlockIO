using System.Text;

namespace BlockIO.GPT
{
    /// <summary>
    /// Represents a single GPT partition entry (128 bytes).
    /// Includes type, GUIDs, LBA range, attributes, and name.
    /// </summary>
    public class GptEntry
    {
        /// <summary>
        /// Gets or sets the partition type GUID (e.g., EFI System, Linux, etc.).
        /// </summary>
        public Guid TypeGuid { get; set; } = Guid.Empty;
        /// <summary>
        /// Gets or sets the unique GUID for this partition.
        /// </summary>
        public Guid UniqueGuid { get; set; } = Guid.Empty;
        /// <summary>
        /// Gets or sets the first LBA of the partition.
        /// </summary>
        public ulong FirstLBA { get; set; } = 0;
        /// <summary>
        /// Gets or sets the last LBA of the partition.
        /// </summary>
        public ulong LastLBA { get; set; } = 0;
        /// <summary>
        /// Gets or sets the partition attributes (bit flags).
        /// </summary>
        public ulong Attributes { get; set; } = 0;
        /// <summary>
        /// Gets or sets the UTF-16 name of the partition (max 36 characters).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether this entry is valid (non-empty type and valid LBA range).
        /// </summary>
        public bool IsValid => TypeGuid != Guid.Empty && FirstLBA <= LastLBA;

        /// <summary>
        /// Serializes the entry to a 128-byte buffer.
        /// </summary>
        /// <returns>A byte array representing the GPT entry.</returns>
        public byte[] ToBytes()
        {
            var buffer = new byte[128];
            TypeGuid.ToByteArray().CopyTo(buffer, 0);
            UniqueGuid.ToByteArray().CopyTo(buffer, 16);
            BitConverter.GetBytes(FirstLBA).CopyTo(buffer, 32);
            BitConverter.GetBytes(LastLBA).CopyTo(buffer, 40);
            BitConverter.GetBytes(Attributes).CopyTo(buffer, 48);
            Encoding.Unicode.GetBytes(Name.PadRight(36, '\0')).CopyTo(buffer, 56);
            return buffer;
        }
        /// <summary>
        /// Parses a GPT entry from a 128-byte buffer.
        /// </summary>
        /// <param name="buffer">The raw 128-byte buffer.</param>
        /// <returns>A populated <see cref="GptEntry"/> instance.</returns>
        public static GptEntry FromBytes(byte[] buffer)
        {
            var entry = new GptEntry
            {
                TypeGuid = new Guid(buffer[0..16]),
                UniqueGuid = new Guid(buffer[16..32]),
                FirstLBA = BitConverter.ToUInt64(buffer, 32),
                LastLBA = BitConverter.ToUInt64(buffer, 40),
                Attributes = BitConverter.ToUInt64(buffer, 48),
                Name = Encoding.Unicode.GetString(buffer, 56, 72).TrimEnd('\0')
            };
            return entry;
        }

        /// <summary>
        /// Returns a concise string representation of the GPT entry for debugging.
        /// </summary>
        /// <returns>A formatted string with key GPT fields.</returns>
        public override string ToString()
        {
            return $"GptEntry: TypeGuid={TypeGuid}, UniqueGuid={UniqueGuid}, FirstLBA={FirstLBA}, LastLBA={LastLBA}, Attributes={Attributes}, Name=\"{Name}\"";
        }
        /// <summary>
        /// Serializes the GPT entry into a human-readable YAML format.
        /// </summary>
        /// <returns>A YAML-formatted string representing the entry.</returns>
        public string ToYaml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("GptEntry:");
            sb.AppendLine($"  TypeGuid: {TypeGuid}");
            sb.AppendLine($"  UniqueGuid: {UniqueGuid}");
            sb.AppendLine($"  FirstLBA: {FirstLBA}");
            sb.AppendLine($"  LastLBA: {LastLBA}");
            sb.AppendLine($"  Attributes: {Attributes}");
            sb.AppendLine($"  Name: \"{Name}\"");
            return sb.ToString();
        }
        /// <summary>
        /// Parses a GPT entry from a YAML-formatted string.
        /// </summary>
        /// <param name="yaml">The YAML string containing GPT entry data.</param>
        /// <returns>A populated <see cref="GptEntry"/> instance.</returns>
        public static GptEntry FromYaml(string yaml)
        {
            var entry = new GptEntry();
            var lines = yaml.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var parts = trimmedLine.Split([':'], 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"');
                    switch (key)
                    {
                        case "TypeGuid":
                            entry.TypeGuid = Guid.Parse(value);
                            break;
                        case "UniqueGuid":
                            entry.UniqueGuid = Guid.Parse(value);
                            break;
                        case "FirstLBA":
                            entry.FirstLBA = ulong.Parse(value);
                            break;
                        case "LastLBA":
                            entry.LastLBA = ulong.Parse(value);
                            break;
                        case "Attributes":
                            entry.Attributes = ulong.Parse(value);
                            break;
                        case "Name":
                            entry.Name = value;
                            break;
                    }
                }
            }
            return entry;
        }
    }
}
