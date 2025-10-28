// SPDX-License-Identifier: EUPL-1.2
// This file is part of the BlockIO project.
// Copyright © 2025 Amber-Sophia Schröck <ambersophia.schroeck@gmail.com>

using BlockIO.Arch;
using BlockIO.Arch.Windows;
using BlockIO.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BlockIO
{
    public class PartitionStream : Stream, IBlockStream
    {
        private AbstractPartition _partition;
        private FileAccess _access;
        private long _position;
        private ulong _length;
        private ulong _syslength;

        /// <summary>
        /// Controls whether read/write operations must be aligned to the current BlockSize.
        /// If true, unaligned lengths will throw an exception.
        /// Default is false (no enforcement).
        /// </summary>
        public bool EnforceLengthAlignment { get; set; } = false;

        #region Stream Overrides
        public PartitionStream(AbstractPartition partition, FileAccess access)
        {
            _partition = partition;
            _access = access;
            _position = 0;
            _syslength = (ulong)(partition.SectorCount * (uint)partition.SectorSize);
            _length = _syslength;
        }

        public override bool CanRead => (_access.HasFlag(FileAccess.Read) && _partition.Readable);
        public override bool CanWrite => (_access.HasFlag(FileAccess.Write) && _partition.Writable);
        public override bool CanSeek => true;

        public override long Length => (long)_length;
        public override long Position { get => (_position); set => _position = value; }
        public int BlockSize { get => _partition.SectorSize;  }

        public override void Flush()
        {
            ArchPartitionStream.FlushDevice(_partition.DevicePath);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(CanRead == false)
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


        public override void Write(byte[] buffer, int offset, int count)
        {
            if(CanWrite == false)
                throw new UnauthorizedAccessException("The partition does not support write access.");

            if (EnforceLengthAlignment && (count % ArchPartitionStream.BlockSize != 0))
                throw new ArgumentException("Write length must be aligned to BlockSize.");

            long partitionOffset = (long)_partition.StartSector * _partition.SectorSize + _position;

            if(isValidPosition(partitionOffset) == false)
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(CanSeek == false)    
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

            if(_position < 0)
                _position = 0;
            if(_position > Length)
                _position = Length;

            return _position;
        }
        
        protected bool isValidPosition(long position)
        {
            return position >= 0 && position <= Length;
        }

        public override void SetLength(long value)
        { 
            if(value < 0 || value > (long)_syslength)
                throw new ArgumentOutOfRangeException(nameof(value), "The specified length is out of partition bounds.");
            _length = (ulong)value;
        }
        #endregion

        public void Reset()         {
            _length = _syslength;
            _position = 0;
        }

        #region IBlockStream Implementation
        public int GetCurrentBlockSize()
        {
            return _partition.SectorSize;
        }

        public Stream CreateSubStream()
        {
            return new PartitionStream(_partition, _access);
        }  
        public Stream CreateSubStream(ulong offset, ulong? length, FileAccess? access)
        {
            if( offset + length > _syslength)
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
