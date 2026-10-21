using SilverCraft.CSCore.DSP;
using SilverCraft.CSCore.Utils;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class DspTests
{
    [Fact]
    public void WindowFunctions_HammingAndHanning_BehaveAsExpected()
    {
        const int width = 101;
        // Hanning at 0 should be 0, at middle (index 50) close to 1
        var hanningStart = WindowFunctions.Hanning(0, width);
        var hanningMid = WindowFunctions.Hanning(50, width);
        Assert.Equal(0.0f, hanningStart, 2);
        Assert.True(hanningMid > 0.95f);

        // Hamming at 0 is ~0.08, at middle close to 1
        var hammingStart = WindowFunctions.Hamming(0, width);
        var hammingMid = WindowFunctions.Hamming(50, width);
        Assert.Equal(0.08f, hammingStart, 2);
        Assert.True(hammingMid > 0.95f);

        // None window function is always 1
        Assert.Equal(1.0f, WindowFunctions.None(0, width));
        Assert.Equal(1.0f, WindowFunctions.None(50, width));
    }

    [Fact]
    public void FastFourierTransformation_ForwardAndBackward_ReconstructsSignal()
    {
        const int exponent = 6; // 2^6 = 64
        var count = 1 << exponent;

        var original = new Complex[count];
        var data = new Complex[count];

        for (var i = 0; i < count; i++)
        {
            var value = (float)Math.Sin(2 * Math.PI * i / 16.0);
            original[i] = new Complex(value, 0);
            data[i] = original[i];
        }

        // Forward FFT
        FastFourierTransformation.Fft(data, exponent, FftMode.Forward);

        // Backward FFT
        FastFourierTransformation.Fft(data, exponent, FftMode.Backward);

        // Verify reconstruction
        for (var i = 0; i < count; i++)
        {
            Assert.Equal(original[i].Real, data[i].Real, 3);
            Assert.Equal(0.0f, data[i].Imaginary, 3);
        }
    }

    [Fact]
    public void LowpassFilter_AttenuatesHighFrequenciesMoreThanLowFrequencies()
    {
        const int sampleRate = 44100;
        const double cutoff = 1000.0;
        var filter = new LowpassFilter(sampleRate, cutoff);

        // 100 Hz low frequency signal
        const int numSamples = 1000;
        var lowFreq = new float[numSamples];
        for (var i = 0; i < numSamples; i++)
        {
            lowFreq[i] = (float)Math.Sin(2 * Math.PI * 100 * i / sampleRate);
        }

        // 10000 Hz high frequency signal
        var highFreq = new float[numSamples];
        for (var i = 0; i < numSamples; i++)
        {
            highFreq[i] = (float)Math.Sin(2 * Math.PI * 10000 * i / sampleRate);
        }

        filter.Process(lowFreq);

        var filter2 = new LowpassFilter(sampleRate, cutoff);
        filter2.Process(highFreq);

        // Compute RMS of steady-state (last 500 samples)
        var lowRms = CalculateRms(lowFreq.AsSpan(500));
        var highRms = CalculateRms(highFreq.AsSpan(500));

        // Low frequency should pass through with high amplitude (~0.7 RMS for unit sine)
        Assert.True(lowRms > 0.65f);
        // High frequency should be heavily attenuated (< 0.05 RMS)
        Assert.True(highRms < 0.05f);
    }

    [Fact]
    public void HighpassFilter_AttenuatesLowFrequenciesMoreThanHighFrequencies()
    {
        const int sampleRate = 44100;
        const double cutoff = 5000.0;
        var filter = new HighpassFilter(sampleRate, cutoff);

        const int numSamples = 1000;
        var lowFreq = new float[numSamples];
        for (var i = 0; i < numSamples; i++)
        {
            lowFreq[i] = (float)Math.Sin(2 * Math.PI * 200 * i / sampleRate);
        }

        var highFreq = new float[numSamples];
        for (var i = 0; i < numSamples; i++)
        {
            highFreq[i] = (float)Math.Sin(2 * Math.PI * 15000 * i / sampleRate);
        }

        filter.Process(lowFreq);

        var filter2 = new HighpassFilter(sampleRate, cutoff);
        filter2.Process(highFreq);

        var lowRms = CalculateRms(lowFreq.AsSpan(500));
        var highRms = CalculateRms(highFreq.AsSpan(500));

        // Low frequency should be heavily attenuated
        Assert.True(lowRms < 0.05f);
        // High frequency should pass through
        Assert.True(highRms > 0.65f);
    }

    private static float CalculateRms(ReadOnlySpan<float> samples)
    {
        var sum = 0.0;
        foreach (var sample in samples)
        {
            sum += sample * sample;
        }
        return (float)Math.Sqrt(sum / samples.Length);
    }
}
