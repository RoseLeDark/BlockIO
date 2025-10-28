// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.Interface
{
    public interface IBlockStream
    {
        int BlockSize { get; }
        int GetCurrentBlockSize();

        Stream CreateSubStream();
        Stream CreateSubStream(ulong offset, ulong? length, FileAccess? access);

    }
}
