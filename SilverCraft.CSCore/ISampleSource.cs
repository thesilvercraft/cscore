namespace SilverCraft.CSCore
{
    /// <summary>
    ///     Defines the base for all audio streams which provide samples instead of raw byte data.
    /// </summary>
    /// <remarks>
    ///     Compared to the <see cref="IWaveSource" />, the <see cref="ISampleSource" /> provides samples instead of raw bytes.
    ///     That means that the <see cref="IAudioSource.Length" /> and the <see cref="IAudioSource.Position" /> properties
    ///     are expressed in samples.
    ///     Also the <see cref="IReadableAudioSource{T}.Read" /> method provides samples instead of raw bytes.
    /// </remarks>
    public interface ISampleSource : IReadableAudioSource<float>
    {
        
    }

    /// <summary>
    ///     Defines the base for all <see cref="ISampleSource" /> aggregators.
    /// </summary>
    public interface ISampleAggregator : ISampleSource, IAggregator<float, ISampleSource>
    {
    }

    /// <summary>
    ///     Defines the base for all <see cref="IWaveSource" /> aggregators.
    /// </summary>
    public interface IWaveAggregator : IWaveSource, IAggregator<byte, IWaveSource>
    {
    }

    /// <summary>
    ///     Defines the base for all aggregators.
    /// </summary>
    /// <typeparam name="T">The type of data, the aggregator provides.</typeparam>
    /// <typeparam name="TAggregator">The type of the aggreator type.</typeparam>
    public interface IAggregator<in T, out TAggregator>
        : IReadableAudioSource<T> where TAggregator : IReadableAudioSource<T>
    {
        /// <summary>
        ///     Gets the underlying <see cref="IReadableAudioSource{T}" />.
        /// </summary>
        /// <value>
        ///     The underlying <see cref="IReadableAudioSource{T}" />.
        /// </value>
        TAggregator BaseSource { get; }
    }
}