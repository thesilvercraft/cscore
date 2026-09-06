namespace SilverCraft.CSCore.Codecs.FLAC
{
    /// <summary>
    /// Provides data for a FlacPreScan.
    /// </summary>
    public class FlacPreScanFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the a list of found frames by the scan.
        /// </summary>
        public IReadOnlyCollection<FlacFrameInformation> Frames { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacPreScanFinishedEventArgs"/> class.
        /// </summary>
        /// <param name="frames">Found frames.</param>
        public FlacPreScanFinishedEventArgs(IReadOnlyCollection<FlacFrameInformation> frames)
        {
            ArgumentNullException.ThrowIfNull(frames);
            Frames = frames;
        }
    }
}