// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.Interface.License
{
    public enum LicenseType
    {
        MIT,
        GPLv2,
        GPLv3,
        LGPLv2_1,
        LGPLv3,
        Apache20,
        BSD2,
        BSD3,
        MPL20,
        EUPL12,
        AGPLv3,
        Unlicense,
        CC0,
        Proprietary,
        BSD4,
        FDLv1_2,
        FDLv1_3,
        QPL,
        WTFPL,
        Zlib,
        Apache11,

        Custom, // For user-defined licenses
    }
    public static class LicenseInfo
    {
        private static readonly Dictionary<LicenseType, bool> m_gplCompatibility = new()
        {
            [LicenseType.MIT] = true,
            [LicenseType.GPLv2] = true,
            [LicenseType.GPLv3] = true,
            [LicenseType.LGPLv2_1] = true,
            [LicenseType.LGPLv3] = true,
            [LicenseType.Apache20] = true,
            [LicenseType.BSD2] = true,
            [LicenseType.BSD3] = true,
            [LicenseType.MPL20] = true,
            [LicenseType.EUPL12] = true,
            [LicenseType.AGPLv3] = true,
            [LicenseType.Unlicense] = true,
            [LicenseType.CC0] = true,
            [LicenseType.Proprietary] = false,
            [LicenseType.Custom] = false // Default für nicht näher spezifizierte
        };

        private static readonly Dictionary<string, bool> m_customLicenses = new(StringComparer.OrdinalIgnoreCase);

        public static bool IsGPLCompatible(LicenseType license, string? customName = null)
        {
            if (license != LicenseType.Custom)
                return m_gplCompatibility.TryGetValue(license, out var compatible) && compatible;

            if (customName is null)
                return false;

            return m_customLicenses.TryGetValue(customName, out var customCompatible) && customCompatible;
        }

        public static void AddCustomLicense(string name, bool isGplCompatible)
        {
            if (!string.IsNullOrWhiteSpace(name))
                m_customLicenses[name] = isGplCompatible;
        }

        public static IEnumerable<string> GetCustomLicenseNames() => m_customLicenses.Keys;

        public static void ClearCustomLicenses() => m_customLicenses.Clear();

        public static bool HasCustomLicense(string name) => m_customLicenses.ContainsKey(name);

        public static void ToYaml(LicenseType license, System.IO.TextWriter writer, string? customName = null)
        {
            if (license != LicenseType.Custom)
            {
                writer.WriteLine($"License: {license}");
            }
            else
            {
                writer.WriteLine($"License: Custom");
                if (customName is not null)
                    writer.WriteLine($"CustomLicenseName: {customName}");
            }
            writer.WriteLine($"GPLCompatible: {IsGPLCompatible(license, customName)}");
        }
    }
}
