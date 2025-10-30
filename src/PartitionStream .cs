// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch;
using BlockIO.Interface;
using System.Runtime.InteropServices;

namespace BlockIO
{
    /// <summary>
    /// Provides a block-oriented stream for reading and writing raw partition data.
    /// Wraps an <see cref="AbstractPartition"/> and enforces structural boundaries and alignment.
    /// </summary>
    public class PartitionStream : Stream, IBlockStream
    {
        private AbstractPartition _partition;
        private FileAccess _access;
        private long _position;
        private ulong _length;
        private ulong _syslength;

        public string DevicePath { get { return _partition.DevicePath; } }

        public int SectorSize { get { return _partition.SectorSize; } }

        public AbstractDevice Device { get { return _partition.Device; } }

        /// <summary>
        /// Controls whether read/write operations must be aligned to the current BlockSize.
        /// If true, unaligned lengths will throw an exception.
        /// Default is false (no enforcement).
        /// </summary>
        public bool EnforceLengthAlignment { get; set; } = false;

        /// <summary>
        /// Gets the block size used for alignment and chunked I/O.
        /// </summary>
        public int BlockSize { get => _partition.SectorSize; }

        /// <summary>
        /// Initializes a new stream for the specified partition and access mode.
        /// </summary>
        /// <param name="partition">The partition to access.</param>
        /// <param name="access">The desired access mode.</param>
        public PartitionStream(AbstractPartition partition, FileAccess access)
        {
            _partition = partition;
            _access = access;
            _position = 0;
            _syslength = (ulong)(partition.SectorCount * (uint)partition.SectorSize);
            _length = _syslength;
        }
        #region Stream Overrides
        /// <inheritdoc/>
        public override bool CanRead => (_access.HasFlag(FileAccess.Read) && _partition.Readable);
        /// <inheritdoc/>
        public override bool CanWrite => (_access.HasFlag(FileAccess.Write) && _partition.Writable);
        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override long Length => (long)_length;
        /// <inheritdoc/>
        public override long Position { get => (_position); set => _position = value; }

        /// <inheritdoc/>
        public override void Flush()
        {
            ArchPartitionStream.FlushDevice(_partition.DevicePath);
        }
        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CanRead == false)
                throw new UnauthorizedAccessException("The partition does not support read access.");

            if (EnforceLengthAlignment && (count % ArchPartitionStream.BlockSize != 0))
                throw new ArgumentException("Read length must be aligned to BlockSize.");

            long partitionOffset = (long)_partition.StartSector * _partition.SectorSize + _position;
            int bytesToRead = Math.Min(count, (int)(Length - _position));

            if (bytesToRead <= 0) return 0;

            ArchPartitionStream.ReadBytes(
                _partition.DevicePath,
                partitionOffset,
                buffer.AsSpan(offset, bytesToRead),
                _partition.SectorSize
            );
            _position += (uint)bytesToRead;

            return bytesToRead;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CanWrite == false)
                throw new UnauthorizedAccessException("The partition does not support write access.");

            if (EnforceLengthAlignment && (count % ArchPartitionStream.BlockSize != 0))
                throw new ArgumentException("Write length must be aligned to BlockSize.");

            long partitionOffset = (long)_partition.StartSector * _partition.SectorSize + _position;

            if (isValidPosition(partitionOffset) == false)
                throw new ArgumentOutOfRangeException(nameof(offset), "The write operation exceeds the partition boundaries.");

            uint bytesToWrite = Math.Min((uint)count, ((uint)Length - (uint)_position));

            if (bytesToWrite <= 0) return;

            ArchPartitionStream.WriteBytes(
                _partition.DevicePath,
                (long)partitionOffset,
                buffer.AsSpan(offset, (int)bytesToWrite),
                _partition.SectorSize
            );
        }
        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (CanWrite == false)
                throw new UnauthorizedAccessException("The partition does not support write access.");

            if (EnforceLengthAlignment && (buffer.Length % ArchPartitionStream.BlockSize != 0))
                throw new ArgumentException("Write length must be aligned to BlockSize.");

            long partitionOffset = (long)_partition.StartSector * _partition.SectorSize + _position;
            if (isValidPosition(partitionOffset) == false)

                throw new ArgumentOutOfRangeException(nameof(partitionOffset), "The write operation exceeds the partition boundaries.");
            uint bytesToWrite = Math.Min((uint)buffer.Length, ((uint)Length - (uint)_position));

            if (bytesToWrite <= 0) return;
            ArchPartitionStream.WriteBytes(
                _partition.DevicePath,
                (long)partitionOffset,
                buffer.Slice(0, (int)bytesToWrite),
                _partition.SectorSize
            );
            _position += bytesToWrite;
        }
        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            if (CanWrite == false)
                throw new UnauthorizedAccessException("The partition does not support write access.");
            long partitionOffset = (long)_partition.StartSector * _partition.SectorSize + _position;
            if (isValidPosition(partitionOffset) == false)
                throw new ArgumentOutOfRangeException(nameof(partitionOffset), "The write operation exceeds the partition boundaries.");
            ArchPartitionStream.WriteBytes(
                _partition.DevicePath,
                (long)partitionOffset,
                MemoryMarshal.CreateSpan(ref value, 1),
                _partition.SectorSize
            );
            _position += 1;
        }
        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (CanSeek == false)
                throw new NotSupportedException("The partition does not support seeking.");

            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = Length + offset;
                    break;
            }

            if (_position < 0)
                _position = 0;
            if (_position > Length)
                _position = Length;

            return _position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            if (value < 0 || value > (long)_syslength)
                throw new ArgumentOutOfRangeException(nameof(value), "The specified length is out of partition bounds.");
            _length = (ulong)value;
        }
        #endregion

        /// <summary>
        /// Validates whether the given position is within the stream bounds.
        /// </summary>
        /// <param name="position">The byte offset to validate.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        protected bool isValidPosition(long position)
        {
            return position >= 0 && position <= Length;
        }

        /// <summary>
        /// Resets the stream position and length to the full partition range.
        /// </summary>
        public void Reset()
        {
            _length = _syslength;
            _position = 0;
        }

        #region IBlockStream Implementation
        /// <inheritdoc/>
        public int GetCurrentBlockSize()
        {
            return _partition.SectorSize;
        }
        /// <inheritdoc/>
        public Stream CreateSubStream()
        {
            return new PartitionStream(_partition, _access);
        }
        /// <inheritdoc/>
        public Stream CreateSubStream(ulong offset, ulong? length, FileAccess? access)
        {
            if (offset + length > _syslength)
                throw new ArgumentOutOfRangeException("The specified offset and length are out of partition bounds.");

            var stream = new PartitionStream(_partition, access.HasValue ? access.Value : _access);
            stream.Seek((long)offset, SeekOrigin.Begin);

            if ((length.HasValue))
            {
                stream.SetLength((long)length.Value);
            }

            return stream;
        }
        #endregion
    }
}
