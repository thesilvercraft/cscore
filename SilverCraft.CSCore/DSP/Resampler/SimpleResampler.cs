namespace SilverCraft.CSCore.DSP.Resampler
{
    /// <summary>
    /// Linear interpolation method.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="SimpleResampler" /> class.
    /// </remarks>
    /// <param name="source">The input sample source.</param>
    /// <param name="sampleRate">Output sampling rate</param>
    public sealed class SimpleResampler(ISampleSource source, int sampleRate) : ResamplerBase(source, sampleRate)
    {
        private float[] _processBuffer = new float[8];

        /// <summary>
        /// Linear-Interpolated Resampling
        /// </summary>
        /// <param name="output">output buffer</param>
        /// <param name="offset">required offset</param>
        /// <param name="count">required count</param>
        /// <returns></returns>
        protected override int Process(float[] output, int offset, int count)
        {
            var channels = WaveFormat.Channels;

            var targetSampleOut = count / channels;
            var samplesToRead = (int)Math.Ceiling(targetSampleOut * InverseConversionRatio) * channels;
            samplesToRead -= samplesToRead % channels;
            _processBuffer = _processBuffer.CheckBuffer(samplesToRead);
            var read = BaseSource.Read(_processBuffer, 0, samplesToRead);
            if (read == 0)
            {
                return 0;
            }

            var samplesIn = read / channels;

            var actualSampleOut = Math.Min(targetSampleOut,
                (int)Math.Floor((samplesIn - 1) / InverseConversionRatio) + 1);
            if (actualSampleOut <= 0 && samplesIn > 0)
            {
                actualSampleOut = 1;
            }

            for (var j = 0; j < actualSampleOut; j++)
            {
                var pos = j * InverseConversionRatio;
                var indexF = (int)Math.Floor(pos);
                var indexC = Math.Min(indexF + 1, samplesIn - 1);
                var frac = (float)(pos - indexF);
                for (var i = 0; i < channels; i++)
                {
                    var sampleF = _processBuffer[i + channels * indexF];
                    var sampleC = _processBuffer[i + channels * indexC];

                    output[offset + i + j * channels] = sampleF + frac * (sampleC - sampleF);
                }
            }

            return actualSampleOut * channels;
        }
    }
}