using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    /// <summary>
    /// Represents a collection of GPT partition entries (128-byte structures).
    /// Provides methods for creation, serialization, and access.
    /// </summary>
    public class GptEntryArray
    {
        /// <summary>
        /// Internal list of GPT entries.
        /// </summary>
        private List<GptEntry> Entries { get; } = [];

        /// <summary>
        /// Gets the number of valid entries in the array.
        /// </summary>
        public int Count => Entries.Count;

        /// <summary>
        /// Gets the number of remaining slots (max 128).
        /// </summary>
        public int FreeSlots => 128 - Entries.Count;

        /// <summary>
        /// Creates an empty GPT entry array with the specified number of blank entries.
        /// </summary>
        /// <param name="count">The number of entries to initialize (typically 128).</param>
        /// <returns>A new <see cref="GptEntryArray"/> instance.</returns>
        public static GptEntryArray CreateEmpty(int count)
        {
            var array = new GptEntryArray();
            for (int i = 0; i < count; i++)
                array.Entries.Add(new GptEntry());
            return array;
        }
        /// <summary>
        /// Initializes a new, empty GPT entry array.
        /// </summary>
        public GptEntryArray() { }

        /// <summary>
        /// Retrieves the entry at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the entry.</param>
        /// <returns>The <see cref="GptEntry"/> at the given index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if index is out of bounds.</exception>
        public GptEntry GetEntry(int index)
        {
            if (index < 0 || index >= Entries.Count)
                throw new IndexOutOfRangeException("GPT entry index out of range.");
            return Entries[index];
        }

        /// <summary>
        /// Returns all entries in the array.
        /// </summary>
        /// <returns>An enumerable of <see cref="GptEntry"/>.</returns>
        public IEnumerable<GptEntry> GetAllEntries()
        {
            return Entries;
        }

        /// <summary>
        /// Parses a GPT entry array from raw bytes.
        /// Only valid entries (non-empty TypeGuid) are included.
        /// </summary>
        /// <param name="buffer">The raw byte buffer containing GPT entries.</param>
        /// <returns>A populated <see cref="GptEntryArray"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if buffer size is not a multiple of 128.</exception>
        public static GptEntryArray FromBytes(byte[] buffer)
        {
            if (buffer.Length % 128 != 0)
                throw new ArgumentException("Invalid GPT entry array size.");
            var array = new GptEntryArray();

            for (int i = 0; i < buffer.Length; i += 128)
            {
                var entryBytes = new byte[128];
                Array.Copy(buffer, i, entryBytes, 0, 128);
                var entry = GptEntry.FromBytes(entryBytes);
                if (entry.IsValid)
                    array.Entries.Add(entry);
            }
            return array;
        }


        /// <summary>
        /// Adds a new GPT entry to the array.
        /// </summary>
        /// <param name="entry">The entry to add.</param>
        /// <exception cref="ArgumentException">Thrown if entry is invalid or duplicate.</exception>
        /// <exception cref="InvalidOperationException">Thrown if array is full.</exception>
        public void AddEntry(GptEntry entry)
        {
            if(!entry.IsValid)
                throw new ArgumentException("Invalid GPT entry.");
            if (Entries.Count >= 128)
                throw new InvalidOperationException("GPT entry array is full.");
            if (Entries.Exists(e => e.UniqueGuid == entry.UniqueGuid))
                throw new ArgumentException("Duplicate UniqueGuid in GPT entries.");

            Entries.Add(entry);
        }

        /// <summary>
        /// Serializes all entries to a contiguous byte array.
        /// </summary>
        /// <returns>A byte array representing all entries.</returns>
        public byte[] ToBytes()
        {
            var buffer = new byte[Entries.Count * 128];
            for (int i = 0; i < Entries.Count; i++)
            {
                var entryBytes = Entries[i].ToBytes();
                Array.Copy(entryBytes, 0, buffer, i * 128, 128);
            }
            return buffer;
        }

        /// <summary>
        /// Serializes the GPT entry array into a human-readable YAML format.
        /// </summary>
        /// <returns>A YAML-formatted string representing all entries.</returns>
        public string ToYaml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("GptEntryArray:");
            for (int i = 0; i < Entries.Count; i++)
            {
                sb.AppendLine($"  - Index: {i}");
                sb.Append(Entries[i].ToYaml().Replace("\n", "\n    "));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a concise string representation of the GPT entry array for debugging.
        /// </summary>
        /// <returns>A formatted string with key GPT fields per entry.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("GptEntryArray:");
            foreach (var entry in Entries)
            {
                sb.AppendLine($"  GptEntry: TypeGuid={entry.TypeGuid}, UniqueGuid={entry.UniqueGuid}, FirstLBA={entry.FirstLBA}, LastLBA={entry.LastLBA}, Attributes={entry.Attributes}, Name=\"{entry.Name}\"");
            }
            return sb.ToString();
        }


        /// <summary>
        /// Parses a GPT entry array from a YAML-formatted string.
        /// Only valid entries are included.
        /// </summary>
        /// <param name="yaml">The YAML string containing GPT entry data.</param>
        /// <returns>A populated <see cref="GptEntryArray"/> instance.</returns>
        public static GptEntryArray FromYaml(string yaml)
        {
            var array = new GptEntryArray();
            var lines = yaml.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            GptEntry? currentEntry = null;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith('-'))
                {
                    if (currentEntry != null && currentEntry.IsValid)
                    {
                        array.AddEntry(currentEntry);
                    }
                    currentEntry = new GptEntry();
                }
                else if (currentEntry != null)
                {
                    var parts = trimmedLine.Split([':'], 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim().Trim('"');
                        switch (key)
                        {
                            case "TypeGuid":
                                currentEntry.TypeGuid = Guid.Parse(value);
                                break;
                            case "UniqueGuid":
                                currentEntry.UniqueGuid = Guid.Parse(value);
                                break;
                            case "FirstLBA":
                                currentEntry.FirstLBA = ulong.Parse(value);
                                break;
                            case "LastLBA":
                                currentEntry.LastLBA = ulong.Parse(value);
                                break;
                            case "Attributes":
                                currentEntry.Attributes = ulong.Parse(value);
                                break;
                            case "Name":
                                currentEntry.Name = value;
                                break;
                        }
                    }
                }
            }
            if (currentEntry != null && currentEntry.IsValid)
            {
                array.AddEntry(currentEntry);
            }
            return array;
        }
        public void ToYamlFile(string path)
        {
            File.WriteAllText(path, ToYaml());
        }
        public static GptEntryArray FromYamlFile(string path)
        {
            var yaml = File.ReadAllText(path);
            return FromYaml(yaml);
        }

    }
}
