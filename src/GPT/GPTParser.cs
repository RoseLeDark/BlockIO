// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch.Windows;
using BlockIO.Interface;
using BlockIO.Interface.License;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace BlockIO.GPT
{

    /// <summary>
    /// Provides an class for parsing GUID Partition Table (GPT) structures from a storage device.
    /// </summary>
    /// <remarks>This class is intended to be inherited by implementations that provide device-specific logic
    /// for determining sector size and parsing GPT partition information. It manages the device path and sector size,
    /// and initializes the partition list by reading the GPT header and entries from the specified device. Derived
    /// classes must implement the method for retrieving the sector size appropriate to the device type.</remarks>
    public class GPTParser  : AbstractParser
    {
        /// <summary>
        /// Represents the size, in bytes, of a sector used by the derived class.
        /// </summary>
        /// <remarks>This field is intended for use by derived classes that require knowledge of the
        /// underlying sector size for storage or data alignment operations.</remarks>
        protected uint m_sectorSize;
        /// <summary>
        /// Stores the file system path to the device associated with this instance.
        /// </summary>
        protected string m_devicePath;
        /// <summary>
        /// Represents the collection of partitions managed by the containing class.
        /// </summary>
        /// <remarks>This field is intended for use by derived classes to access or modify the set of
        /// partitions. Modifying this collection directly may affect the behavior of partition-related operations in
        /// the containing class.</remarks>
        protected List<AbstractPartition> m_partitions = [];

        /// <summary>
        /// Gets the device path used to identify the device instance.
        /// </summary>
        public override string Path { get => m_devicePath; }

        /// <summary>
        /// Gets the size, in bytes, of a single sector on the underlying storage device.
        /// </summary>
        public override uint SectorSize { get => m_sectorSize; }

        public GPTParser()
        {
            m_devicePath = string.Empty;
        }

        public GPTParser(string devicePath)
        {
            m_devicePath = devicePath;
        }
        
        public List<AbstractPartition> GetPartitions() 
        {
            return [.. m_partitions];  
        }


        /// <summary>
        /// Gets the total number of partitions currently managed by the instance.
        /// </summary>
        public override int PartitionCount { get; internal set; }

        public override string Name => "System GPT Parser";

        public override VersionInfo Version => VersionInfo.Parse("1.0.0");

        public override string Author => "BlockIO Team";

        public override LicenseType License => LicenseType.EUPL12;

        public override string Description => "Provides an class for parsing GUID Partition Table (GPT) structures from a storage device.";

        protected override bool CanParse(Stream stream, ref string errorString)
        {
            // GPT Header lesen
            stream.Seek(m_sectorSize, SeekOrigin.Begin);
            var header = new byte[m_sectorSize];
            _ = stream.Read(header, 0, (int)m_sectorSize);

            // GPT Signatur prüfen
            var signature = System.Text.Encoding.ASCII.GetString(header, 0, 8);
            if (signature != "EFI PART")
            {
                errorString = "Kein GPT Header gefunden."; // "No GPT header found."
                return false;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!WindowsDriveInfo.CanParse(m_devicePath, ref errorString))
                    return false;
            }


            return true;
        }
        public override List<AbstractPartition> parse(AbstractDevice device, uint sectorSize = 0)
        {
            using var stream = new FileStream(m_devicePath, FileMode.Open, FileAccess.Read);

            string errorString = string.Empty;
            if (CanParse(stream, ref errorString) == false) 
                throw new InvalidOperationException(errorString);
            
            m_sectorSize = sectorSize == 0 ? GetSectorSize(m_devicePath) : sectorSize;

            m_partitions.Clear();

            // GPT Header lesen
            stream.Seek(m_sectorSize, SeekOrigin.Begin);
            var header = new byte[m_sectorSize];
            _ = stream.Read(header, 0, (int)m_sectorSize);

            GptHeader _gptheader = GptHeader.FromBytes(header);

            // Wichtige Felder aus dem Header extrahieren
            ulong partitionEntryLBA = _gptheader.PartitionEntryLBA;
            uint numEntries = _gptheader.NumberOfEntries;
            uint entrySize = _gptheader.EntrySize;

            // Zu den Partitionseinträgen springen
            stream.Seek((long)partitionEntryLBA * m_sectorSize, SeekOrigin.Begin);

            // Partitionseinträge auslesen
            for (int i = 0; i < numEntries; i++)    
            {
                
                var entry = new byte[entrySize];
                _ = stream.Read(entry, 0, (int)entrySize);

                GptEntry _gptentry = GptEntry.FromBytes(entry);
                if(_gptentry.IsValid == false)  
                    continue;


                // Partition zur Liste hinzufügen
                m_partitions.Add(new GPTPartition(device, _gptentry.Name, 
                    _gptentry.TypeGuid, _gptentry.UniqueGuid, 
                    _gptentry.FirstLBA, _gptentry.LastLBA, 
                    (int)m_sectorSize));
            }
            PartitionCount = m_partitions.Count;

            // Kopie der Gefundene Partitionen zurückgeben
            return [.. m_partitions];
        }



        protected override uint GetSectorSize(string path)
        {
            uint _sektorSize = 512; // Standard Sektorgröße

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string errorString = string.Empty;
                _sektorSize = WindowsDriveInfo.GetSectorSize(path, ref errorString);
                if(_sektorSize == 0)
                    throw new InvalidOperationException(errorString);
            }
            return _sektorSize;
        }
    }
}
