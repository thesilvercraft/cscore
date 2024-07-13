using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SilverCraft.CSCore.SoundOut.AL;

namespace SilverCraft.CSCore.SoundOut
{
	/// <summary>
	///     Provides audioplayback through OpenAL.
	/// </summary>
	/// <remarks>
	/// This SoundOut provider runs on multiple platforms. 
	/// But since the OpenAL implementation on Windows platforms, has some different
	/// handling what context switching concerns, it is not possible to play sounds on multiple 
	/// devices at once through OpenAL. 
	/// </remarks>
	// ReSharper disable once InconsistentNaming
	public class ALSoundOut : ISoundOut
	{
		/// <summary>
		/// Checks whether the OpenAL library can be found and loaded
		/// </summary>
        public static bool IsSupported => ALInteropsNativeMethods.IsSupported();
        private const int NumberOfBuffers = 4;
		private readonly object _lockObj = new object();
		private readonly ThreadPriority _playbackPriority;
		private readonly SynchronizationContext _syncContext;
		private ALSource _alSource;
		private uint[] _buffers;
		private int _bufferSize;

		private ALDevice _device;
		private bool _disposed;
		private bool _isInitialized;
		private int _latency;
		private ALFormat _playbackFormat;
		private PlaybackState _playbackState;

		private Thread _playbackThread;
		private ALDevice _playingDevice;
		private IWaveSource _source;
		private ALContext _context;

		/// <summary>
		///     Initializes a new instance of the <see cref="ALSoundOut" /> class.
		/// </summary>
		public ALSoundOut()
			: this(50)
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="ALSoundOut" /> class with a initial latency
        ///     and <see cref="ThreadPriority" /> of the playback thread.
        /// </summary>
        /// <param name="latency">The playback latency in milliseconds.</param>
        /// <param name="playbackThreadPriority">The <see cref="ThreadPriority" /> of the playback thread.</param>
        /// <exception cref="ArgumentOutOfRangeException">latency</exception>
        public ALSoundOut(int latency, ThreadPriority playbackThreadPriority = ThreadPriority.AboveNormal)
			: this(latency, playbackThreadPriority, SynchronizationContext.Current)
		{
		}

        /// <summary>
        ///     Initializes a new instance of the <see cref="ALSoundOut" /> class based on a initial latency,
        ///     the <see cref="ThreadPriority" /> of the playback thread and the <see cref="SynchronizationContext" /> used to
        ///     raise events.
        /// </summary>
        /// <param name="latency">The playback latency in milliseconds.</param>
        /// <param name="playbackThreadPriority">The <see cref="ThreadPriority" /> of the playback thread.</param>
        /// <param name="eventSyncContext">
        ///     The <see cref="SynchronizationContext" /> which is used to raise any events like the <see cref="Stopped" />-event.
        ///     If the passed value is not null, the events will be called async through the
        ///     <see cref="SynchronizationContext.Post" /> method.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">latency</exception>
        public ALSoundOut(int latency, ThreadPriority playbackThreadPriority, SynchronizationContext eventSyncContext)
		{
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(latency);

            _latency = latency;
			_playbackPriority = playbackThreadPriority;
			_syncContext = eventSyncContext;
			if(!ResolverIsSet)
			{
				ResolverIsSet = true;
                NativeLibrary.SetDllImportResolver(typeof(ALSoundOut).Assembly, ALInteropsNativeMethods.DllImportResolver);
            }

            if (!ALInteropsNativeMethods.IsSupported())
            {
                throw new PlatformNotSupportedException("openAL is not supported by the current platform. Consider installing openAL on the current platform.");
            }
		}
        /// <summary>
        /// Has ALSoundOut been called previously, if so skip setting the DLL import resolver
        /// </summary>
        private static bool ResolverIsSet = false;
        /// <summary>
        ///     Gets or sets the <see cref="Device" /> which should be used for playback.
        ///     The <see cref="Device" /> property has to be set before initializing.
        ///     The systems default playback device is used as default
        ///     value of the <see cref="Device" /> property.
        /// </summary>
        /// <exception cref="ArgumentNullException">value is less than one</exception>
        public ALDevice Device
		{
			get => _device ?? (ALDevice.DefaultDevice);
			set
			{
                ArgumentNullException.ThrowIfNull(value);
                lock (_lockObj)
				{
					_device = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the latency of the playback specified in milliseconds.
		///     The <see cref="Latency" /> property has to be set before initializing.
		/// </summary>
		public int Latency
		{
			get => _latency;
			set
			{
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                lock (_lockObj)
				{
					_latency = value;
				}
			}
		}

	
		/// <summary>
		///     Gets or sets the volume of the playback.
		///     Valid values are in the range from 0.0 (0%) to 1.0 (100%).
		/// </summary>
		public float Volume {
			get {
				return _alSource != null ? _alSource.Gain : 1;
			}
			set {
				CheckForDisposed();
				CheckForIsInitialized();
				_alSource.Gain = value;
			}
		}
		/// <summary>
		///     Gets the <see cref="IWaveSource" /> which provides
		///     the waveform-audio data and was used to <see cref="Initialize" />
		///     the <see cref="ALSoundOut" /> instance.
		/// </summary>
		public IWaveSource WaveSource => _source;

		/// <summary>
		///     Gets the <see cref="SoundOut.PlaybackState" />.
		///     The playback state indicates whether the playback is currently playing, paused or stopped.
		/// </summary>
		public PlaybackState PlaybackState => _playbackState;

		/// <summary>
		///     Gets the Context used for the playback.
		/// </summary>
		protected ALContext Context => _context;

		/// <summary>
		///     Occurs when the playback stops.
		/// </summary>
		public event EventHandler<PlaybackStoppedEventArgs> Stopped;

        /// <summary>
        ///     Starts the playback.
        ///     Note: The <see cref="Initialize" /> method has to get called before calling <see cref="Play" />.
        ///     If the <see cref="PlaybackState" /> is <see cref="PlaybackState.Paused" />, the
        ///     <see cref="Resume" />
        ///     will be called automatically.
        /// </summary>
        public void Play()
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				CheckForDisposed();
				CheckForIsInitialized();

				switch (PlaybackState)
				{
					case PlaybackState.Stopped:
					{
						using var waitHandle = new ManualResetEvent(false);
						_playbackThread.WaitForExit();
						_playbackThread = new Thread(PlaybackProc)
						{
							Name = "CSCORE OpenAL Playback",
							Priority = _playbackPriority
						};

						_playbackThread.Start(waitHandle);
						waitHandle.WaitOne();
						break;
					}
					case PlaybackState.Paused:
						Resume();
						break;
					case PlaybackState.Playing:
					default:
						break;
				}
			}
		}

		/// <summary>
		///     Pauses the audio playback.
		/// </summary>
		public void Pause()
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				CheckForDisposed();
				CheckForIsInitialized();

				if (PlaybackState != PlaybackState.Playing) return;
				_alSource.Pause();
				_playbackState = PlaybackState.Paused;
			}
		}

		/// <summary>
		///     Resumes the audio playback.
		/// </summary>
		public void Resume()
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				CheckForDisposed();
				CheckForIsInitialized();

				if (PlaybackState != PlaybackState.Paused) return;
				_alSource.Play();
				_playbackState = PlaybackState.Playing;
			}
		}

		/// <summary>
		///     Stops the audio playback and releases most of allocated resources.
		/// </summary>
		public void Stop()
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				CheckForDisposed();
				//don't check for isinitialized here (we don't want the Dispose method to throw an exception)

				if (PlaybackState != PlaybackState.Stopped)
				{
					_alSource?.Stop();

					_playbackState = PlaybackState.Stopped;
				}

				if (_playbackThread == null) return;
				/*
				 * On EOF playbackstate is Stopped, but thread is not stopped. =>
				 * New Session can be started while cleaning up old one => unknown behavior. =>
				 * Always call Stop() to make sure, you wait until the thread is finished cleaning up.
				 */
				_playbackThread.WaitForExit();
				_playbackThread = null;
			}
		}

        /// <summary>
        ///     Initializes the <see cref="ALSoundOut" /> instance for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source"><see cref="IWaveSource" /> which provides waveform-audio data to play.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="PlaybackState" /> is not
        ///     <see cref="PlaybackState.Stopped" />.
        /// </exception>
        public void Initialize(IWaveSource source)
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				CheckForDisposed();

                ArgumentNullException.ThrowIfNull(source);

                source = new InterruptDisposingChainSource(source);
				if (PlaybackState != PlaybackState.Stopped)
				{
					throw new InvalidOperationException(
						"PlaybackState has to be Stopped. Call ALSoundOut.Stop to stop the playback.");
				}

				//wait for the playbackthread to finish
				_playbackThread.WaitForExit();
				//after the playbackthread finished, release the resources 
				CleanupResources();
				//start creating new resources including new context and so on.
				_playingDevice = Device;
				_context = new ALContext(_playingDevice);

				source = new InterruptDisposingChainSource(source);

				var numberOfBitsPerSample = FindBestBitDepth(source.WaveFormat);
				_source = source.ToSampleSource().ToWaveSource(numberOfBitsPerSample);

				InitializeInternal();

				_isInitialized = true;
			}
		}

		/// <summary>
		///     Stops the playback (if playing) and releases all allocated resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void PlaybackProc(object args)
		{
			Exception exception = null;
			var waitHandle = args as EventWaitHandle;
			IList<BufferedAudioData> byteBuffers;
			uint[] unqueuedBuffers;

			using (Context.LockContext())
			{
				//if we run eof, and we did not call initialize, the buffers are still queued
				//make sure the buffers are unququed before trying to fill them
				if (_alSource.BuffersQueued == 0 && _alSource.BuffersProcessed == 0)
				{
					unqueuedBuffers = _buffers;
				}
				else
				{
					while ((unqueuedBuffers = _alSource.UnqueueBuffers(_alSource.BuffersProcessed)).Length <= 0)
					{
						Thread.Sleep(Latency / 5);
					}
				}
			}

			if ((byteBuffers = GetBufferedData(unqueuedBuffers.Length)).Count <= 0)
			{
				_playbackState = PlaybackState.Stopped;
			}
			else
			{
				using (Context.LockContext())
				{

					FillBuffers(unqueuedBuffers, byteBuffers);
					_alSource.Play();

					_playbackState = PlaybackState.Playing;
					if (waitHandle != null)
					{
						waitHandle.Set();
						waitHandle = null;
					}
				}
			}

			try
			{
				while (PlaybackState != PlaybackState.Stopped)
				{
					if (PlaybackState == PlaybackState.Paused)
					{
						Thread.Sleep(Latency / 5);
						continue;
					}

					//locks and unlocks context!
					var numberOfProcessedBuffers = _alSource.BuffersProcessed;
					if (numberOfProcessedBuffers == 0)
					{
						Thread.Sleep(Latency / 5);
						continue;
					}

					if ((byteBuffers = GetBufferedData(numberOfProcessedBuffers)).Count <= 0)
					{
						_playbackState = PlaybackState.Stopped;
					}
					else
					{
						using (Context.LockContext())
						{
							unqueuedBuffers = _alSource.UnqueueBuffers(numberOfProcessedBuffers);
							FillBuffers(unqueuedBuffers, byteBuffers);

							//locks and unlocks context!
							if (_alSource.SourceState == ALSourceState.Stopped)
								_alSource.Play();
						}
					}
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				_playbackState = PlaybackState.Stopped;

				waitHandle?.Set();

				RaiseStopped(exception);
			}
		}

		private void RaiseStopped(Exception exception)
		{
			var handler = Stopped;
			if (handler == null) return;
			if (_syncContext != null)
				_syncContext.Post(x => handler(this, new PlaybackStoppedEventArgs(exception)), null);
			else
				handler(this, new PlaybackStoppedEventArgs(exception));
		}

		private void InitializeInternal()
		{
			using (Context.LockContext())
			{
				_playbackFormat = FindALFormat(_source.WaveFormat);
				_alSource = new ALSource(Context);

				_buffers = new uint[NumberOfBuffers];
				ALException.Try(
					() =>
					ALInteropsNativeMethods.alGenBuffers(_buffers.Length, _buffers),
					"alGenBuffers");
			}
			_bufferSize = (int)_source.WaveFormat.MillisecondsToBytes(_latency);
		}

		private void CleanupResources()
		{
			if (!_isInitialized)
				return;

			if (_alSource != null)
			{
				using (Context.LockContext())
				{
					var numberOfProcessedBuffers = _alSource.BuffersProcessed;
					if (numberOfProcessedBuffers > 0)
					{
						//sometimes there are duplicates on window??
						var finishedBuffers = _alSource.UnqueueBuffers(numberOfProcessedBuffers).Distinct().ToArray();
						ALException.Try(
							() =>
							ALInteropsNativeMethods.alDeleteBuffers(finishedBuffers.Length, finishedBuffers),
							"alDeleteBuffers");
					}

					_alSource.Dispose();
					_alSource = null;
				}
			}

			if (Context != null)
			{
				Context.Dispose();
				_context = null;
			}

			_isInitialized = false;
		}

		private IList<BufferedAudioData> GetBufferedData(int numberOfBuffers)
		{
			var byteBuffers = new List<BufferedAudioData>(numberOfBuffers);
			for (var i = 0; i < numberOfBuffers; i++)
			{
				var buffer = new byte[_bufferSize];
				var read = _source.Read(buffer, 0, buffer.Length);
				if (read <= 0)
				{
					continue;
				}

				byteBuffers.Add(new BufferedAudioData()
					{
						Data = buffer,
						Length = read
					});
			}

			return byteBuffers;
		}

		private void FillBuffers(uint[] buffers, IList<BufferedAudioData> audioData)
		{
			for (var i = 0; i < buffers.Length; i++)
			{
				FillBuffer(buffers[i], audioData[i].Data, audioData[i].Length);
			}
		}

		private void FillBuffer(uint bufferHandle, byte[] buffer, int count)
		{
			using (Context.LockContext())
			{
				ALException.Try(
					() =>
					ALInteropsNativeMethods.alBufferData(bufferHandle, _playbackFormat, buffer, count,
						(uint) _source.WaveFormat.SampleRate),
					"alBufferData");
				_alSource.QueueBuffer(bufferHandle);
			}
		}

		/// <summary>
		///     Disposes and stops the <see cref="ALSoundOut" /> instance.
		/// </summary>
		/// <param name="disposing">
		///     True to release both managed and unmanaged resources; false to release only unmanaged
		///     resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			CheckForInvalidThreadCall();

			lock (_lockObj)
			{
				if (!_disposed)
				{
					Debug.WriteLine("Disposing ALSoundOut");
					Stop();
					CleanupResources();
				}
				_disposed = true;
			}
		}

		~ALSoundOut()
		{
			Dispose(false);
		}

		private int FindBestBitDepth(WaveFormat waveFormat)
		{
			var bitsPerSample = waveFormat.BitsPerSample;
			var supportedBitsPerSample = new[]
			{
				8, 
				16, 
				Context.Supports32Float ? 32 : 16
			}.OrderBy(x => x);

			foreach (var bits in supportedBitsPerSample)
			{
				if (bits >= bitsPerSample)
					return bits;
			}

			return supportedBitsPerSample.Max();
		}

		private ALFormat FindALFormat(WaveFormat waveFormat)
		{
			if (waveFormat.Channels == 1)
			{
				return waveFormat.BitsPerSample switch
				{
					8 => ALFormat.Mono8Bit,
					16 => ALFormat.Mono16Bit,
					32 => ALFormat.MonoFloat32Bit,
					64 => ALFormat.MonoDouble,
					_ => throw new ALException("Invalid BitsPerSample.")
				};
			}
			//apparently https://learn.microsoft.com/en-us/dotnet/api/opentk.audio.openal.alformat?view=xamarin-ios-sdk-12 claims there are more ALFormats
			if (waveFormat.Channels != 2) throw new NotImplementedException("Only mono and stereo are implemented by ALSoundOut.");
			return waveFormat.BitsPerSample switch
			{
				8 => ALFormat.Stereo8Bit,
				16 => ALFormat.Stereo16Bit,
				32 => ALFormat.StereoFloat32Bit,
				64 => ALFormat.StereoDouble,
				_ => throw new ALException("Invalid BitsPerSample.")
			};
		}

		private void CheckForInvalidThreadCall()
		{
			if (Thread.CurrentThread == _playbackThread)
				throw new InvalidOperationException("You must not access this method from the PlaybackThread.");
		}

		private void CheckForDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("ALSoundOut");
		}

		private void CheckForIsInitialized()
		{
			if (!_isInitialized)
				throw new InvalidOperationException("ALSoundOut is not initialized.");
		}

		private class InterruptDisposingChainSource : WaveAggregatorBase
		{
			public InterruptDisposingChainSource(IWaveSource source)
				: base(source)
			{
                ArgumentNullException.ThrowIfNull(source);
                DisposeBaseSource = false;
			}
		}

		private struct BufferedAudioData
		{
			public byte[] Data;
			public int Length;
		}
	}
}