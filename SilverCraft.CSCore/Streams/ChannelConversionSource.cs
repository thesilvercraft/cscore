using SilverCraft.CSCore.DSP;

namespace SilverCraft.CSCore.Streams
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
        public override int Read(float[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
                throw new ArgumentException("Buffer too small", nameof(buffer));
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            var inChannels = BaseSource.WaveFormat.Channels;
            var outChannels = _channelMatrix.OutputChannelCount;

            count -= count % outChannels;

            var bytesToRead = (int)OutputToInput(count);
            _buffer = _buffer.CheckBuffer(bytesToRead);

            var read = base.Read(_buffer, 0, bytesToRead);

            var frameCount = read / inChannels;
            var outputSampleCount = frameCount * outChannels;

            if (frameCount == 0)
                return 0;

            ReadOnlySpan<float> input = _buffer.AsSpan(0, frameCount * inChannels);
            var output = buffer.AsSpan(offset, outputSampleCount);

            var inIdx = 0;
            var outIdx = 0;

            for (var f = 0; f < frameCount; f++)
            {
                for (var ic = 0; ic < inChannels; ic++)
                {
                    var sampleVal = input[inIdx++];
            
                    // Channel matrix accumulation
                    for (var oc = 0; oc < outChannels; oc++)
                    {
                        output[outIdx + oc] += sampleVal * _channelMatrix[ic, oc].Value;
                    }
                }
                outIdx += outChannels;
            }

            return outputSampleCount;
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
