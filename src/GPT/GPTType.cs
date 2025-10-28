// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    /// <summary>
    /// Enumerates all known GPT partition types, including a placeholder for user-defined entries.
    /// </summary>
    public enum GPTType
    {
        Unused,
        MBRPartitionScheme,
        EFISystem,
        BIOSBoot,
        MicrosoftReserved,
        BasicData,
        LDMMetadata,
        LDMData,
        WindowsRecovery,
        IBMGPFS,
        HP_UXData,
        HP_UXService,
        LinuxFilesystem,
        LinuxSwap,
        LinuxLVM,
        LinuxRAID,
        LinuxReserved,
        FreeBSDBoot,
        FreeBSDData,
        FreeBSDSwap,
        FreeBSDUFS,
        FreeBSDVinum,
        FreeBSDZFS,
        MacOSHFS,
        MacOSAPFS,
        MacOSUFS,
        MacOSBoot,
        MacOSRAID,
        MacOSRAIDSlice,
        MacOSX86,
        MacOSReserved,
        ChromeOSKernel,
        ChromeOSRoot,
        ChromeOSFuture,
        AndroidMeta,
        AndroidExt,
        AndroidCache,
        AndroidData,
        CephOSD,
        VMKCoreDump,
        VMFS,
        VMwareReserved,
        UserDefined,
        VirtualPartitionFromDevice // Internal use: represents full-device virtual partition
    }

    /// <summary>
    /// Provides a registry of known GPT partition types and their associated GUIDs.
    /// Also supports user-defined GUIDs with named registration and YAML serialization.
    /// </summary>
    public static class GPTTypeRegistry
    {
        /// <summary>
        /// Static mapping of official GPT types to their GUIDs.
        /// </summary>
        public static readonly IReadOnlyDictionary<GPTType, Guid> TypeGuids = new Dictionary<GPTType, Guid>
        {
            { GPTType.Unused, Guid.Parse("00000000-0000-0000-0000-000000000000") },
            { GPTType.MBRPartitionScheme, Guid.Parse("024DEE41-33E7-11D3-9D69-0008C781F39F") },
            { GPTType.EFISystem, Guid.Parse("C12A7328-F81F-11D2-BA4B-00A0C93EC93B") },
            { GPTType.BIOSBoot, Guid.Parse("21686148-6449-6E6F-744E-656564454649") },
            { GPTType.MicrosoftReserved, Guid.Parse("E3C9E316-0B5C-4DB8-817D-F92DF00215AE") },
            { GPTType.BasicData, Guid.Parse("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7") },
            { GPTType.LDMMetadata, Guid.Parse("5808C8AA-7E8F-42E0-85D2-E1E90434CFB3") },
            { GPTType.LDMData, Guid.Parse("AF9B60A0-1431-4F62-BC68-3311714A69AD") },
            { GPTType.WindowsRecovery, Guid.Parse("DE94BBA4-06D1-4D40-A16A-BFD50179D6AC") },
            { GPTType.IBMGPFS, Guid.Parse("37AFFC90-EF7D-4E96-91C3-2D7AE055B174") },
            { GPTType.HP_UXData, Guid.Parse("75894C1E-3AEB-11D3-B7C1-7B03A0000000") },
            { GPTType.HP_UXService, Guid.Parse("E2A1E728-32E3-11D6-A682-7B03A0000000") },
            { GPTType.LinuxFilesystem, Guid.Parse("0FC63DAF-8483-4772-8E79-3D69D8477DE4") },
            { GPTType.LinuxSwap, Guid.Parse("0657FD6D-A4AB-43C4-84E5-0933C84B4F4F") },
            { GPTType.LinuxLVM, Guid.Parse("E6D6D379-F507-44C2-A23C-238F2A3DF928") },
            { GPTType.LinuxRAID, Guid.Parse("A19D880F-05FC-4D3B-A006-743F0F84911E") },
            { GPTType.LinuxReserved, Guid.Parse("8DA63339-0007-60C0-C436-083AC8230908") },
            { GPTType.FreeBSDBoot, Guid.Parse("83BD6B9D-7F41-11DC-BE0B-001560B84F0F") },
            { GPTType.FreeBSDData, Guid.Parse("516E7CB4-6ECF-11D6-8FF8-00022D09712B") },
            { GPTType.FreeBSDSwap, Guid.Parse("516E7CB5-6ECF-11D6-8FF8-00022D09712B") },
            { GPTType.FreeBSDUFS, Guid.Parse("516E7CB6-6ECF-11D6-8FF8-00022D09712B") },
            { GPTType.FreeBSDVinum, Guid.Parse("516E7CB8-6ECF-11D6-8FF8-00022D09712B") },
            { GPTType.FreeBSDZFS, Guid.Parse("516E7CBA-6ECF-11D6-8FF8-00022D09712B") },
            { GPTType.MacOSHFS, Guid.Parse("48465300-0000-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSAPFS, Guid.Parse("7C3457EF-0000-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSUFS, Guid.Parse("55465300-0000-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSBoot, Guid.Parse("426F6F74-0000-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSRAID, Guid.Parse("52414944-0000-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSRAIDSlice, Guid.Parse("52414944-5F53-11AA-AA11-00306543ECAC") },
            { GPTType.MacOSX86, Guid.Parse("6A82CB45-1DD2-11B2-99A6-080020736631") },
            { GPTType.MacOSReserved, Guid.Parse("6A85CF4D-1DD2-11B2-99A6-080020736631") },
            { GPTType.ChromeOSKernel, Guid.Parse("FE3A2A5D-4F32-41A7-B725-ACCC3285A309") },
            { GPTType.ChromeOSRoot, Guid.Parse("3CB8E202-3B7E-47DD-8A3C-7FF2A13CFCEC") },
            { GPTType.ChromeOSFuture, Guid.Parse("2E0A753D-9E48-43B0-8337-B15192CB1B5E") },
            { GPTType.AndroidMeta, Guid.Parse("2568845D-2332-4675-BC39-8FA5A4748D15") },
            { GPTType.AndroidExt, Guid.Parse("114EAFFE-1552-4022-B26E-9B053604CF84") },
            { GPTType.AndroidCache, Guid.Parse("A893EF55-2E0E-4C3D-9FE7-3D5A3A2931DF") },
            { GPTType.AndroidData, Guid.Parse("DC76DDA9-5AC1-491C-AF42-A82591580C0D") },
            { GPTType.CephOSD, Guid.Parse("4FBD7E29-9D25-41B8-AFD0-062C0CEFF05D") },
            { GPTType.VMKCoreDump, Guid.Parse("9D275380-40AD-11DB-BF97-000C2911D1B8") },
            { GPTType.VMFS, Guid.Parse("AA31E02A-400F-11DB-9590-000C2911D1B8") },
            { GPTType.VMwareReserved, Guid.Parse("9D275382-40AD-11DB-BF97-000C2911D1B8") },
            { GPTType.UserDefined, Guid.Empty }, // Reserved placeholder for user-defined types

            // System-internal virtual partition GUID (not part of official GPT spec)
            { GPTType.VirtualPartitionFromDevice, Guid.Parse("B10CDEAD-0000-4D3B-A006-743F0F84911E") }, // Internal use: represents full-device virtual partition

        };


        private static readonly ConcurrentDictionary<string, Guid> _userTypes = new();

        /// <summary>
        /// Gets the dictionary of user-defined GPT type names and their GUIDs.
        /// </summary>
        public static IReadOnlyDictionary<string, Guid> UserTypes => _userTypes;

        /// <summary>
        /// Registers a user-defined GPT GUID with a custom name.
        /// </summary>
        /// <param name="guid">The GUID to register.</param>
        /// <param name="name">The name to associate with the GUID.</param>
        /// <returns>The registered name.</returns>
        public static string RegisterUserGuid(Guid guid, string name)
        {
            if (!_userTypes.ContainsKey(name))
                _userTypes.TryAdd(name, guid);
            return name;
        }

        /// <summary>
        /// Retrieves the name associated with a user-defined GUID.
        /// </summary>
        /// <param name="guid">The GUID to look up.</param>
        /// <returns>The associated name, or null if not found.</returns>
        public static string? GetUsertName(Guid guid)
        {
            foreach (var kvp in _userTypes)
            {
                if (kvp.Value.Equals(guid))
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the GUID associated with a user-defined name.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The associated GUID, or null if not found.</returns>
        public static Guid? GetUserGuid(string name)
        {
            if (_userTypes.TryGetValue(name, out var guid))
                return guid;
            return null;
        }

        /// <summary>
        /// Removes a user-defined GUID by name.
        /// </summary>
        /// <param name="name">The name to unregister.</param>
        /// <returns>True if removed successfully; otherwise false.</returns>
        public static bool UnregisterUserGuid(string name)
        {
            return _userTypes.TryRemove(name, out _);
        }

        /// <summary>
        /// Clears all user-defined GUID registrations.
        /// </summary>
        public static void ClearUserGuids()
        {
            _userTypes.Clear();
        }

        /// <summary>
        /// Determines whether a GUID is registered as user-defined.
        /// </summary>
        /// <param name="guid">The GUID to check.</param>
        /// <returns>True if user-defined; otherwise false.</returns>
        public static bool IsUserDefined(Guid guid)
        {
            return GetUsertName(guid) != null;
        }

        /// <summary>
        /// Serializes all user-defined GUIDs to a YAML-formatted string.
        /// </summary>
        /// <returns>YAML representation of user-defined GUIDs.</returns>
        public static string ToYAML()
        {
            var sb = new StringBuilder();

            if (_userTypes.Count > 0)
            {
                sb.AppendLine("UserDefinedGuids:");
                foreach (var kvp in _userTypes)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes user-defined GUIDs from a YAML-formatted string.
        /// </summary>
        /// <param name="yaml">The YAML input string.</param>
        public static void FromYAML(string yaml)
        {
            var lines = yaml.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool inUserDefinedSection = false;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Equals("UserDefinedGuids:", StringComparison.OrdinalIgnoreCase))
                {
                    inUserDefinedSection = true;
                    continue;
                }
                if (inUserDefinedSection)
                {
                    if (trimmedLine.StartsWith("-") || string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        inUserDefinedSection = false;
                        continue;
                    }
                    var parts = trimmedLine.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        var name = parts[0].Trim();
                        var guidString = parts[1].Trim();
                        if (Guid.TryParse(guidString, out var guid))
                        {
                            RegisterUserGuid(guid, name);
                        }
                    }
                }
            }
        }
    }
}