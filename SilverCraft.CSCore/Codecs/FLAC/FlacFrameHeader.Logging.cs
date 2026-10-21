using Microsoft.Extensions.Logging;

namespace SilverCraft.CSCore.Codecs.FLAC;

public partial class FlacFrameHeader
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Not able to read Flac header - EOF?")]
    private partial void Log_EOFErrorHeader();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid FlacFrame. Reservedbit_0 is 1")]
    private partial void Log_EOFErrorReservedBit1();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid Blocksize value: 0")]
    private partial void Log_EOFErrorBlocksize0();

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Missing SampleRate. SampleRate Index = 0 && streamInfoMetaData == null")]
    private partial void Log_EOFMissingSampleRate();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid SampleRate value: {Value}")]
    private partial void Log_EOFInvalidSampleRate(int value);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid ChannelAssignment")]
    private partial void Log_EOFInvalidChannelAssignment();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Missing BitsPerSample. Index = 0 && streamInfoMetaData == null")]
    private partial void Log_MissingBitsPerSample();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid BitsPerSampleIndex")]
    private partial void Log_InvalidBitsPerSampleIndex();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid FlacFrame. Reservedbit_1 is 1")]
    private partial void Log_InvalidErrorReservedBit1();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid UTF8 Samplenumber coding")]
    private partial void Log_InvalidUtf8SampleNumberCoding();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid UTF8 Framenumber coding")]
    private partial void Log_InvalidUtf8FrameNumberCoding();

    [LoggerMessage(Level = LogLevel.Debug, Message = "CRC8 missmatch")]
    private partial void Log_ErrorCRC8();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Invalid Syncbits")]
    private partial void Log_InvalidSyncBits();
}