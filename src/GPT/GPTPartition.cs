// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>
using BlockIO.Interface;
using BlockIO.Interface.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    internal class GPTPartition : AbstractPartition
    {
        public GPTPartition(AbstractDevice device, string Name, Guid typeGuid, Guid uniqueGuid, ulong firstSector, ulong lastSector, int sectorSize) 
            : base(device, Name + "_GPT", typeGuid, uniqueGuid, firstSector, lastSector, sectorSize)
        {
        }

        public override VersionInfo Version => VersionInfo.Parse("1.0.0");

        public override string Author => "BlockIO Team";

        public override LicenseType License => LicenseType.EUPL12;

        public override string Description => "";

        protected override void OnCloned(AbstractPartition clone)
        {
            // nothing special to do
        }
    }
}
