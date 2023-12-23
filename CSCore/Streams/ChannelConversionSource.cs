using CSCore.DSP;
using System;

namespace CSCore.Streams
{
    /// <summary>
    /// NOT RELEASED YET! Provides conversion between a set of input and output channels using a <see cref="ChannelMatrix"/>.
    /// </summary>
    internal class ChannelConversionSource : SampleAggregatorBase
    {
        private readonly ChannelMatrix _channelMatrix;
        private WaveFormat _waveFormat;
        private float _ratio;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelConversionSource"/> class.
        /// </summary>
        /// <param name="source">The <see cref="ISampleSource"/> which provides input data.</param>
        /// <param name="channelMatrix">The <see cref="ChannelMatrix"/> which defines the mapping of the input channels to the output channels.</param>
        public ChannelConversionSource(ISampleSource source, ChannelMatrix channelMatrix)
            : base(source)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(channelMatrix);

            _channelMatrix = channelMatrix;
            _waveFormat = channelMatrix.BuildOutputWaveFormat(source);

            _ratio = (float)_waveFormat.Channels / source.WaveFormat.Channels;
        }

        private float[] _buffer;

        /// <summary>
        ///     Reads a sequence of samples from the <see cref="ChannelConversionSource" /> and advances the position within the stream by
        ///     the number of samples read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of floats. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     float array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the floats read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of samples to read from the current source.</param>
        /// <returns>The total number of samples read into the buffer.</returns>
        public override unsafe int Read(float[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new ArgumentException("buffer");
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            count -= count % WaveFormat.Channels;

            var bytesToRead = (int)OutputToInput(count);
            _buffer = _buffer.CheckBuffer(bytesToRead);

            var read = base.Read(_buffer, 0, bytesToRead);
            var counter = 0;
            Array.Clear(buffer, offset, (int) InputToOutput(read));

            fixed (float* buf = &buffer[offset])
            {
                var pbuf = buf;

                for (var i = 0; i < read; i++)
                {
                    var value = _buffer[i];

                    var channelIndex = i % BaseSource.WaveFormat.Channels;
                    for (var o = 0; o < _channelMatrix.OutputChannelCount; o++)
                    {
                        pbuf[o] += value * _channelMatrix[channelIndex, o].Value;
                    }

                    if (channelIndex == _channelMatrix.InputChannelCount - 1 || i == read - 1)
                    {
                        pbuf += _channelMatrix.OutputChannelCount;
                        counter += _channelMatrix.OutputChannelCount;
                    }
                }
            }


            return counter;
        }

        /// <summary>
        /// Gets the output format.
        /// </summary>
        public override WaveFormat WaveFormat => _waveFormat;

        /// <summary>
        /// Gets or sets the position in samples.
        /// </summary>
        public override long Position
        {
            get => InputToOutput(base.Position);

            set => base.Position = OutputToInput(value);
        }

        /// <summary>
        /// Gets the length in samples.
        /// </summary>
        public override long Length => InputToOutput(base.Length);

        internal long InputToOutput(long position)
        {
            var result = (long)(position * _ratio);
            result -= (result % WaveFormat.Channels);
            return result;
        }

        internal long OutputToInput(long position)
        {
            var result = (long)(position / _ratio);
            result -= (result % BaseSource.WaveFormat.Channels);
            return result;
        }
    }
}
