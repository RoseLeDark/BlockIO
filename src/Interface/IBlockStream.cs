// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

namespace BlockIO.Interface
{
    /// <summary>
    /// Defines a block-oriented stream abstraction for accessing structured data segments.
    /// Provides methods to retrieve aligned substreams and query block size configuration.
    /// </summary>
    public interface IBlockStream
    {
        /// <summary>
        /// Gets the block size used for alignment and chunking within the stream.
        /// </summary>
        int BlockSize { get; }
        /// <summary>
        /// Returns the current block size used by the stream implementation.
        /// </summary>
        /// <returns>The block size in bytes.</returns>
        int GetCurrentBlockSize();

        /// <summary>
        /// Creates a substream that spans the entire underlying stream.
        /// </summary>
        /// <returns>A new <see cref="Stream"/> instance representing the full range.</returns>
        Stream CreateSubStream();

        /// <summary>
        /// Creates a substream from the specified offset and optional length, with optional access mode.
        /// </summary>
        /// <param name="offset">The starting offset in bytes.</param>
        /// <param name="length">The optional length of the substream in bytes. If null, spans to the end.</param>
        /// <param name="access">The optional access mode (read, write, or both).</param>
        /// <returns>A new <see cref="Stream"/> instance representing the specified segment.</returns>
        Stream CreateSubStream(ulong offset, ulong? length, FileAccess? access);

    }
}
