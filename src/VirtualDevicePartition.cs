// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.GPT;
using BlockIO.Interface;
using BlockIO.Interface.License;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO
{
    /// <summary>
    /// Represents a virtual partition that spans the entire underlying device.
    /// Used to expose a full-device stream abstraction without relying on physical partition boundaries.
    /// </summary>
    internal class VirtualDevicePartition : AbstractPartition
    {
        /// <summary>
        /// Initializes a new virtual partition over the entire device.
        /// </summary>
        /// <param name="device">The underlying device to wrap.</param>
        public VirtualDevicePartition(AbstractDevice device)
            : base(device, "VDevPart:" + device.Name)        { }

        /// <summary>
        /// Gets the version information for this partition implementation.
        /// </summary>
        public override VersionInfo Version => new VersionInfo(1, 0, 0);

        /// <summary>
        /// Gets the author attribution for this partition implementation.
        /// </summary>
        public override string Author => "BlockIO Library";

        /// <summary>
        /// Gets the license type under which this partition implementation is distributed.
        /// </summary>
        public override LicenseType License => LicenseType.EUPL12;

        /// <summary>
        /// Gets a human-readable description of this partition type.
        /// </summary>
        public override string Description => "A virtual partition representing the entire device.";

        /// <inheritdoc/>
        protected override void OnCloned(AbstractPartition clone)
        {
        }
    }
}