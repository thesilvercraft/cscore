namespace SilverCraft.CSCore
{
    /// <summary>
    ///     Defines the base for all audio streams which provide raw byte data.
    /// </summary>
    /// <remarks>
    ///     Compared to the <see cref="ISampleSource" />, the <see cref="IWaveSource" /> provides raw bytes instead of samples.
    ///     That means that the <see cref="IAudioSource.Position" /> and the <see cref="IAudioSource.Position" /> properties are
    ///     expressed in bytes.
    ///     Also the <see cref="IReadableAudioSource{T}.Read" /> method provides samples instead of raw bytes.
    /// </remarks>
    public interface IWaveSource : IReadableAudioSource<byte>
    {
    }
}