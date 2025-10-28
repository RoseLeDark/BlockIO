// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Interface.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.Interface
{
    public struct VersionInfo
    {
        public int Major;
        public int Minor;
        public int Patch;
        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        public static VersionInfo Default { get => new(1, 0, 0); }

        public VersionInfo()
        {
            Major = 0;
            Minor = 0;
            Patch = 1;
        }

        public VersionInfo(int major)
        {
            Major = major;
            Minor = 0;
            Patch = 0;
        }

        public VersionInfo(int major, int minor)
        {
            Major = major;
            Minor = minor;
            Patch = 0;
        }

        public VersionInfo(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }
        public VersionInfo IncrementMajor() => new VersionInfo(Major + 1, 0, 0);
        public VersionInfo IncrementMinor() => new VersionInfo(Major, Minor + 1, 0);
        public VersionInfo IncrementPatch() => new VersionInfo(Major, Minor, Patch + 1);

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
        public override bool Equals(object? obj)
        {
            if (obj is VersionInfo other)
            {
                return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch);
        }

        public static bool operator ==(VersionInfo v1, VersionInfo v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(VersionInfo v1, VersionInfo v2)
        {
            return !v1.Equals(v2);
        }

        public static bool operator >(VersionInfo v1, VersionInfo v2)
        {
            if (v1.Major != v2.Major)
                return v1.Major > v2.Major;
            if (v1.Minor != v2.Minor)
                return v1.Minor > v2.Minor;
            return v1.Patch > v2.Patch;
        }   

        public static bool operator <(VersionInfo v1, VersionInfo v2)
        {
            if (v1.Major != v2.Major)
                return v1.Major < v2.Major;
            if (v1.Minor != v2.Minor)
                return v1.Minor < v2.Minor;
            return v1.Patch < v2.Patch;
        }

        public static bool operator >=(VersionInfo v1, VersionInfo v2)
        {
            return v1 > v2 || v1 == v2;
        }

        public static bool operator <=(VersionInfo v1, VersionInfo v2)
        {
            return v1 < v2 || v1 == v2;
        }

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

        public void ToYaml(System.IO.TextWriter writer)
        {
            writer.WriteLine($"Version: {Major}.{Minor}.{Patch} ");
        }
    }

    public interface IMetaInfo
    {
        string Name { get; }
        VersionInfo Version { get; }
        string Author { get; }
        LicenseType License { get; }
        string Description { get; }

        /// <summary>
        /// Optional: Only used if License == Other
        /// </summary>
        string? CustomLicenseName => null;

        
    }

}
