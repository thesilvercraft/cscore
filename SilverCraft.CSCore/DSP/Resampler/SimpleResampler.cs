using System;

namespace SilverCraft.CSCore.DSP.Resampler
{
	/// <summary>
	/// Linear interpolation method.
	/// </summary>
	public sealed class SimpleResampler : ResamplerBase
	{
		/// <summary>
		/// Creates a new instance of the <see cref="SimpleResampler" /> class.
		/// </summary>
		/// <param name="source">The input sample source.</param>
		/// <param name="SampleRate">Output sampling rate</param>
		public SimpleResampler(ISampleSource source, int SampleRate) : base(source, SampleRate)
		{
		}

		private float[] processBuffer = new float[8];

		/// <summary>
		/// Linear-Interpolated Resampling
		/// </summary>
		/// <param name="output">output buffer</param>
		/// <param name="offset">required offset</param>
		/// <param name="count">required count</param>
		/// <returns></returns>
		protected override int Process(float[] output, int offset, int count)
		{
			var sampleOut = count / WaveFormat.Channels;
			var samplesToRead = (int)Math.Ceiling(sampleOut * InverseConversionRatio) * WaveFormat.Channels;
			samplesToRead -= samplesToRead % WaveFormat.Channels;
			processBuffer = processBuffer.CheckBuffer(samplesToRead);
			var read = BaseSource.Read(processBuffer, 0, samplesToRead);
			var samplesIn = read / WaveFormat.Channels;
			for (var i = 0; i < WaveFormat.Channels; i++)
			{
				for (var j = 0; j < sampleOut; j++)
				{
					var pos = j * InverseConversionRatio;
					var indexF = (int)Math.Floor(pos);
					var indexC = Math.Min(indexF + 1, samplesIn - 1);
					var ratio = indexC - pos;
					output[i + j * WaveFormat.Channels] = (float)(ratio * processBuffer[i + WaveFormat.Channels * indexF] + (1 - ratio) * processBuffer[i + WaveFormat.Channels * indexC]);
				}
			}
			return count;
		}
	}
}
