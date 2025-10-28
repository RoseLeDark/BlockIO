// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
//
using BlockIO.Interface;
using System;
using System.Collections.Generic;
using System.Text;

#if BLOCKIO_PLUGIN_SUPPORT

namespace BlockIO.Plugin
{
    public interface IBlockIOPlugin
    {
        string Name { get; }
        string Description { get; }
        IEnumerable<AbstractParser> GetParsers();
        IEnumerable<AbstractDevice> GetDevices();

    }

}
#endif