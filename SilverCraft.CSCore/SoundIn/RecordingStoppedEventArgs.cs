using System;

namespace SilverCraft.CSCore.SoundIn
{
    /// <summary>
    ///     Provides data for the <see cref="ISoundIn.Stopped" /> event.
    /// </summary>
    /// <remarks>
    ///     Initializes a new instance of the <see cref="RecordingStoppedEventArgs" /> class.
    /// </remarks>
    /// <param name="exception">The associated exception. Can be null.</param>
    public class RecordingStoppedEventArgs(Exception exception) : StoppedEventArgs(exception)
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RecordingStoppedEventArgs" /> class.
        /// </summary>
        public RecordingStoppedEventArgs()
            : this(null)
        {
        }
    }
}