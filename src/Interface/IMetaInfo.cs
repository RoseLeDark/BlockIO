// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface.License;

namespace BlockIO.Interface
{
    /// <summary>
    /// Represents a semantic version number using Major.Minor.Patch format.
    /// Supports comparison, parsing, and formatting operations.
    /// </summary>
    public struct VersionInfo
    {
        /// <summary>The major version component.</summary>
        public int Major;
        /// <summary>The minor version component.</summary>
        public int Minor;
        /// <summary>The patch version component.</summary>
        public int Patch;

        /// <summary>
        /// Returns the version string in "Major.Minor.Patch" format.
        /// </summary>
        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        /// <summary>
        /// Gets the default version (1.0.0).
        /// </summary>
        public static VersionInfo Default { get => new(1, 0, 0); }

        /// <summary>
        /// Initializes a version with default values (0.0.1).
        /// </summary>
        public VersionInfo()
        {
            Major = 0;
            Minor = 0;
            Patch = 1;
        }
        /// <summary>
        /// Initializes a version with a specified major value. Minor and patch default to 0.
        /// </summary>
        /// <param name="major">The major version number.</param>
        public VersionInfo(int major)
        {
            Major = major;
            Minor = 0;
            Patch = 0;
        }
        /// <summary>
        /// Initializes a version with specified major and minor values. Patch defaults to 0.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        public VersionInfo(int major, int minor)
        {
            Major = major;
            Minor = minor;
            Patch = 0;
        }
        /// <summary>
        /// Initializes a version with specified major, minor, and patch values.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public VersionInfo(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
        /// <summary>
        /// Returns a new version with the major component incremented. Minor and patch reset to 0.
        /// </summary>
        public VersionInfo IncrementMajor() => new VersionInfo(Major + 1, 0, 0);
        /// <summary>
        /// Returns a new version with the minor component incremented. Patch reset to 0.
        /// </summary>
        public VersionInfo IncrementMinor() => new VersionInfo(Major, Minor + 1, 0);
        /// <summary>
        /// Returns a new version with the patch component incremented.
        /// </summary>
        public VersionInfo IncrementPatch() => new VersionInfo(Major, Minor, Patch + 1);

        /// <summary>
        /// Attempts to parse a version string in "Major.Minor.Patch" format.
        /// </summary>
        /// <param name="versionString">The input string to parse.</param>
        /// <param name="versionInfo">The resulting <see cref="VersionInfo"/> if successful.</param>
        /// <returns>True if parsing succeeds; otherwise, false.</returns>
        public static bool TryParse(string versionString, out VersionInfo versionInfo)
        {
            versionInfo = new VersionInfo();
            var parts = versionString.Split('.');
            if (parts.Length != 3)
                return false;
            if (int.TryParse(parts[0], out int major) &&
                int.TryParse(parts[1], out int minor) &&
                int.TryParse(parts[2], out int patch))
            {
                versionInfo = new VersionInfo(major, minor, patch);
                return true;
            }
            return false;
        }
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is VersionInfo other)
            {
                return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
            }
            return false;
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }
        /// <summary>Equality operator.</summary>
        public static bool operator ==(VersionInfo v1, VersionInfo v2)
        {
            return v1.Equals(v2);
        }
        /// <summary>Inequality operator.</summary>
        public static bool operator !=(VersionInfo v1, VersionInfo v2)
        {
            return !v1.Equals(v2);
        }
        /// <summary>Greater-than operator.</summary>
        public static bool operator >(VersionInfo v1, VersionInfo v2)
        {
            if (v1.Major != v2.Major)
                return v1.Major > v2.Major;
            if (v1.Minor != v2.Minor)
                return v1.Minor > v2.Minor;
            return v1.Patch > v2.Patch;
        }
        /// <summary>Less-than operator.</summary>
        public static bool operator <(VersionInfo v1, VersionInfo v2)
        {
            if (v1.Major != v2.Major)
                return v1.Major < v2.Major;
            if (v1.Minor != v2.Minor)
                return v1.Minor < v2.Minor;
            return v1.Patch < v2.Patch;
        }
        /// <summary>Greater-than-or-equal operator.</summary>
        public static bool operator >=(VersionInfo v1, VersionInfo v2)
        {
            return v1 > v2 || v1 == v2;
        }
        /// <summary>Less-than-or-equal operator.</summary>
        public static bool operator <=(VersionInfo v1, VersionInfo v2)
        {
            return v1 < v2 || v1 == v2;
        }
        /// <summary>
        /// Parses a version string in "Major.Minor.Patch" format.
        /// </summary>
        /// <param name="versionString">The input string to parse.</param>
        /// <returns>A new <see cref="VersionInfo"/> instance.</returns>
        /// <exception cref="FormatException">Thrown if the format is invalid.</exception>
        public static VersionInfo Parse(string versionString)
        {
            var parts = versionString.Split('.');
            if (parts.Length != 3)
                throw new FormatException("Invalid version string format. Expected format: Major.Minor.Patch");
            return new VersionInfo(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2])
            );
        }
        /// <summary>
        /// Serializes the version to YAML format.
        /// </summary>
        /// <param name="writer">The text writer to output YAML.</param>
        public void ToYaml(System.IO.TextWriter writer)
        {
            writer.WriteLine($"Version: {Major}.{Minor}.{Patch} ");
        }
    }
    /// <summary>
    /// Defines metadata for a plugin, module, or component.
    /// Includes name, version, author, license, and description.
    /// </summary>
    public interface IMetaInfo
    {
        /// <summary>
        /// The name of the component or plugin.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The semantic version of the component.
        /// </summary>
        VersionInfo Version { get; }
        /// <summary>
        /// The author or maintainer of the component.
        /// </summary>
        string Author { get; }
        /// <summary>
        /// The license type under which the component is published.
        /// </summary>
        LicenseType License { get; }
        /// <summary>
        /// A brief description of the component's purpose or functionality.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Optional: Only used if License == Other
        /// </summary>
        string? CustomLicenseName => null;

    }

}
