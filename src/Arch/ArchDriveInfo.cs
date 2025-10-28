// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using BlockIO.Interface;

namespace BlockIO.Arch
{
    internal class ArchDriveInfo
    {
        internal static uint GetSectorSize(string path, ref string errorstring)
        {
            uint sektorSize = 64; // Standard Sektorgröße
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                sektorSize = WindowsDriveInfo.GetSectorSize(path, ref errorstring);
            }
            return sektorSize;
        }
        internal static bool CanParse(string path, ref string errorString) 
        {             
            bool canParse = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                canParse = WindowsDriveInfo.CanParse(path, ref errorString);
            }
            return canParse;
        }
        public static DeviceType GetDeviceType(string path)
        {
            DeviceType deviceType = DeviceType.Unknown;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                deviceType = WindowsDriveInfo.GetDeviceType(path);
            }
            return deviceType;
        }
        internal static ulong GetMaxLBA(string devicePath, ref string errorString)
        {             
            ulong maxLBA = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                maxLBA = WindowsDriveInfo.GetMaxLBA(devicePath, ref errorString);
            }
            return maxLBA;
        }
    }
}