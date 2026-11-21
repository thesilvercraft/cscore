using Microsoft.Extensions.Logging;

namespace SilverCraft.CSCore.Codecs.FLAC;

public partial class FlacFrameHeader
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Not able to read Flac header - EOF?")]
    private static partial void Log_EOFErrorHeader(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid FlacFrame. Reservedbit_0 is 1")]
    private static partial void Log_EOFErrorReservedBit1(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid Blocksize value: 0")]
    private static partial void Log_EOFErrorBlocksize0(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Missing SampleRate. SampleRate Index = 0 && streamInfoMetaData == null")]
    private static partial void Log_EOFMissingSampleRate(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid SampleRate value: {Value}")]
    private static partial void Log_EOFInvalidSampleRate(ILogger logger, int value);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid ChannelAssignment")]
    private static partial void Log_EOFInvalidChannelAssignment(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Missing BitsPerSample. Index = 0 && streamInfoMetaData == null")]
    private static partial void Log_MissingBitsPerSample(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid BitsPerSampleIndex")]
    private static partial void Log_InvalidBitsPerSampleIndex(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid FlacFrame. Reservedbit_1 is 1")]
    private static partial void Log_InvalidErrorReservedBit1(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid UTF8 Samplenumber coding")]
    private static partial void Log_InvalidUtf8SampleNumberCoding(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid UTF8 Framenumber coding")]
    private static partial void Log_InvalidUtf8FrameNumberCoding(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "CRC8 missmatch")]
    private static partial void Log_ErrorCRC8(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid Syncbits")]
    private static partial void Log_InvalidSyncBits(ILogger logger);
}