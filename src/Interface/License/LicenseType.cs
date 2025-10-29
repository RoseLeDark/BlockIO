// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

namespace BlockIO.Interface.License
{
    /// <summary>
    /// Represents known software license identifiers supported by BlockIO.
    /// Includes common open-source licenses, permissive and copyleft variants, and custom declarations.
    /// </summary>
    public enum LicenseType
    {
        /// <summary>MIT License</summary>
        MIT,

        /// <summary>GNU General Public License v2</summary>
        GPLv2,

        /// <summary>GNU General Public License v3</summary>
        GPLv3,

        /// <summary>GNU Lesser General Public License v2.1</summary>
        LGPLv2_1,

        /// <summary>GNU Lesser General Public License v3</summary>
        LGPLv3,

        /// <summary>Apache License 2.0</summary>
        Apache20,

        /// <summary>BSD 2-Clause License</summary>
        BSD2,

        /// <summary>BSD 3-Clause License</summary>
        BSD3,

        /// <summary>Mozilla Public License 2.0</summary>
        MPL20,

        /// <summary>European Union Public License v1.2</summary>
        EUPL12,

        /// <summary>GNU Affero General Public License v3</summary>
        AGPLv3,

        /// <summary>The Unlicense (public domain dedication)</summary>
        Unlicense,

        /// <summary>Creative Commons Zero (CC0)</summary>
        CC0,

        /// <summary>Proprietary license (non-free)</summary>
        Proprietary,

        /// <summary>BSD 4-Clause License</summary>
        BSD4,

        /// <summary>GNU Free Documentation License v1.2</summary>
        FDLv1_2,

        /// <summary>GNU Free Documentation License v1.3</summary>
        FDLv1_3,

        /// <summary>Q Public License</summary>
        QPL,

        /// <summary>Do What The F*** You Want To Public License</summary>
        WTFPL,

        /// <summary>Zlib License</summary>
        Zlib,

        /// <summary>Apache License 1.1</summary>
        Apache11,

        /// <summary>User-defined license not covered by known identifiers</summary>
        Custom
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

        /// <summary>
        /// Determines whether the specified license is GPL-compatible.
        /// </summary>
        /// <param name="license">The license type to evaluate.</param>
        /// <param name="customName">Optional name for custom licenses.</param>
        /// <returns>True if the license is GPL-compatible; otherwise, false.</returns>
        public static bool IsGPLCompatible(LicenseType license, string? customName = null)
        {
            if (license != LicenseType.Custom)
                return m_gplCompatibility.TryGetValue(license, out var compatible) && compatible;

            if (customName is null)
                return false;

            return m_customLicenses.TryGetValue(customName, out var customCompatible) && customCompatible;
        }

        /// <summary>
        /// Registers a custom license name and its GPL compatibility status.
        /// </summary>
        /// <param name="name">The name of the custom license.</param>
        /// <param name="isGplCompatible">True if the license is GPL-compatible.</param>
        public static void AddCustomLicense(string name, bool isGplCompatible)
        {
            if (!string.IsNullOrWhiteSpace(name))
                m_customLicenses[name] = isGplCompatible;
        }

        /// <summary>
        /// Returns all registered custom license names.
        /// </summary>
        /// <returns>An enumerable of custom license names.</returns>
        public static IEnumerable<string> GetCustomLicenseNames() => m_customLicenses.Keys;

        /// <summary>
        /// Clears all registered custom licenses.
        /// </summary>
        public static void ClearCustomLicenses() => m_customLicenses.Clear();

        /// <summary>
        /// Checks whether a custom license with the given name is registered.
        /// </summary>
        /// <param name="name">The name of the custom license.</param>
        /// <returns>True if the license is registered; otherwise, false.</returns>
        public static bool HasCustomLicense(string name) => m_customLicenses.ContainsKey(name);

        /// <summary>
        /// Serializes license metadata to YAML format.
        /// </summary>
        /// <param name="license">The license type to serialize.</param>
        /// <param name="writer">The output writer for YAML content.</param>
        /// <param name="customName">Optional name for custom licenses.</param>
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
