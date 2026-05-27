#define GET_BUFFER_INTERNAL

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Represents a frame inside of an Flac-Stream.
    /// </summary>
    public sealed partial class FlacFrame : IDisposable
    {
        private List<FlacSubFrameData> _subFrameData;
#if FLAC_DEBUG
        private System.Collections.ObjectModel.ReadOnlyCollection<FlacSubFrameBase> _subFrames;  
#endif
        private Stream _stream;
        private FlacMetadataStreamInfo _streamInfo;

        private GCHandle _handle1, _handle2;
        private int[]? _destBuffer;
        private int[]? _residualBuffer;

        /// <summary>
        /// Gets the header of the flac frame.
        /// </summary>
        public FlacFrameHeader Header { get; private set; }

        /// <summary>
        /// Gets the CRC16-checksum.
        /// </summary>
        public short Crc16 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the decoder has encountered an error with this frame.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this frame contains an error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FlacFrame"/> class based on the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream which contains the flac frame.</param>
        /// <returns>A new instance of the <see cref="FlacFrame"/> class.</returns>
        public static FlacFrame FromStream(Stream stream)
        {
            var frame = new FlacFrame(stream);
            return frame;
            //return frame.HasError ? null : frame;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FlacFrame"/> class based on the specified <paramref name="stream"/> and some basic stream information.
        /// </summary>
        /// <param name="stream">The stream which contains the flac frame.</param>
        /// <param name="streamInfo">Some basic information about the flac stream.</param>
        /// <returns>A new instance of the <see cref="FlacFrame"/> class.</returns>
        public static FlacFrame FromStream(Stream stream, FlacMetadataStreamInfo streamInfo)
        {
            var frame = new FlacFrame(stream, streamInfo);
            return frame;
            //return frame.HasError ? null : frame;
        }

        private FlacFrame(Stream stream, FlacMetadataStreamInfo? streamInfo = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (stream.CanRead == false) 
                throw new ArgumentException("Stream is not readable");

            _stream = stream;
            _streamInfo = streamInfo;
        }

        /// <summary>
        /// Tries to read the next flac frame inside of the specified stream and returns a value which indicates whether the next flac frame could be successfully read.
        /// </summary>
        /// <returns>True if the next flac frame could be successfully read; false if not.</returns>
        public bool NextFrame()
        {
            Decode(_stream, _streamInfo);
            return !HasError;
        }

        private void Decode(Stream stream, FlacMetadataStreamInfo streamInfo)
        {
            Header = new FlacFrameHeader(stream, streamInfo);
            _stream = stream;
            _streamInfo = streamInfo;
            HasError = Header.HasError;
            if (HasError) return;
            ReadSubFrames();
            FreeBuffers();
        }

        private unsafe void ReadSubFrames()
        {
            var subFrames = new List<FlacSubFrameBase>();

            //alocateOutput
            var data = AllocOutputMemory();
            _subFrameData = data;

          var buffer = new byte[0x20000];
          if ((_streamInfo.MaxFrameSize * Header.Channels * Header.BitsPerSample * 2 >> 3) > buffer.Length)
          {
              buffer = new byte[(_streamInfo.MaxFrameSize * Header.Channels * Header.BitsPerSample * 2 >> 3) - FlacConstant.FrameHeaderSize];
          }
          var read = _stream.Read(buffer, 0, (int)Math.Min(buffer.Length, _stream.Length - _stream.Position));

          fixed (byte* ptrBuffer = buffer)
          {
              using var reader = new FlacBitReader(ptrBuffer, 0);
              for (var c = 0; c < Header.Channels; c++)
              {
                  var bitsPerSample = Header.BitsPerSample;
                  switch (Header.ChannelAssignment)
                  {
                      case ChannelAssignment.MidSide or ChannelAssignment.LeftSide:
                          bitsPerSample += c;
                          break;
                      case ChannelAssignment.RightSide:
                          bitsPerSample += 1 - c;
                          break;
                  }

                  var subframe = FlacSubFrameBase.GetSubFrame(reader, data[c], Header, bitsPerSample);
                  subFrames.Add(subframe);
              }

              reader.Flush(); //Zero-padding to byte alignment.

              //footer
              Crc16 = (short) reader.ReadBits(16);

              _stream.Position -= read - reader.Position;
              MapToChannels(_subFrameData);
          }
#if FLAC_DEBUG
            _subFrames = subFrames.AsReadOnly();
#endif
        }

        private unsafe void MapToChannels(List<FlacSubFrameData> subFrames)
        {
            switch (Header.ChannelAssignment)
            {
                case ChannelAssignment.LeftSide:
                {
                    for (var i = 0; i < Header.BlockSize; i++)
                    {
                        subFrames[1].DestinationBuffer[i] = subFrames[0].DestinationBuffer[i] - subFrames[1].DestinationBuffer[i];
                    }

                    break;
                }
                case ChannelAssignment.RightSide:
                {
                    for (var i = 0; i < Header.BlockSize; i++)
                    {
                        subFrames[0].DestinationBuffer[i] += subFrames[1].DestinationBuffer[i];
                    }

                    break;
                }
                case ChannelAssignment.MidSide:
                {
                    for (var i = 0; i < Header.BlockSize; i++)
                    {
                        var mid = subFrames[0].DestinationBuffer[i] << 1;
                        var side = subFrames[1].DestinationBuffer[i];

                        mid |= (side & 1);

                        subFrames[0].DestinationBuffer[i] = (mid + side) >> 1;
                        subFrames[1].DestinationBuffer[i] = (mid - side) >> 1;
                    }

                    break;
                }
            }
        }




        private unsafe List<FlacSubFrameData> AllocOutputMemory()
        {
            if (_destBuffer == null || _destBuffer.Length < (Header.Channels * Header.BlockSize))
                _destBuffer = new int[Header.Channels * Header.BlockSize];
            if (_residualBuffer == null || _residualBuffer.Length < (Header.Channels * Header.BlockSize))
                _residualBuffer = new int[Header.Channels * Header.BlockSize];

            var output = new List<FlacSubFrameData>();

            for (var c = 0; c < Header.Channels; c++)
            {
                fixed (int* ptrDestBuffer = _destBuffer, ptrResidualBuffer = _residualBuffer)
                {
                    _handle1 = GCHandle.Alloc(_destBuffer, GCHandleType.Pinned);
                    _handle2 = GCHandle.Alloc(_residualBuffer, GCHandleType.Pinned);

                    var data = new FlacSubFrameData
                    {
                        DestinationBuffer = (ptrDestBuffer + c * Header.BlockSize),
                        ResidualBuffer = (ptrResidualBuffer + c * Header.BlockSize)
                    };
                    output.Add(data);
                }
            }

            return output;
        }

        private void FreeBuffers()
        {
            if (_handle1.IsAllocated)
                _handle1.Free();
            if (_handle2.IsAllocated)
                _handle2.Free();
        }

        private bool _disposed;

        /// <summary>
        /// Disposes the <see cref="FlacFrame"/> and releases all associated resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            GC.SuppressFinalize(this);
            FreeBuffers();
            _destBuffer = null;
            _residualBuffer = null;
            
            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FlacFrame"/> class.
        /// </summary>
        ~FlacFrame()
        {
            Dispose();
        }
    }
}