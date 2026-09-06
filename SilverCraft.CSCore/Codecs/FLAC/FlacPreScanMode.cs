namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Defines how to scan a flac stream.
    /// </summary>
    public enum FlacPreScanMode
    {
        /// <summary>
        /// Don't scan the flac stream. This will cause a stream to be not seekable if it has no valid SeekTable.
        /// </summary>
        None,

        /// <summary>
        /// Scan synchronously. Scanning is skipped if a valid SeekTable is found.
        /// </summary>
        Sync,

        /// <summary>
        /// Scan async. Scanning is skipped if a valid SeekTable is found, .
        /// </summary>
        /// <remarks>
        /// Don't use the stream while scan is running because the stream position
        /// will change while scanning. If you playback the stream, it will cause an error!
        /// </remarks>
        Async,

        /// <summary>
        /// Default value.
        /// </summary>
        Default = Sync,
    }
}