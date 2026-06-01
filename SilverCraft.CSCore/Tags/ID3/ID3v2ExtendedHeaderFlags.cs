using System;

namespace SilverCraft.CSCore.Tags.ID3
{
    /// <summary>
    /// Flags indicating various options and restrictions found in the extended header section of an ID3v2 tag.
    /// </summary>
    [Flags]
    public enum ID3v2ExtendedHeaderFlags
    {
        None = 0x0,
        TagUpdate = 0x4,
        CrcPresent = 0x2,
        Restrict = 0x1
    }
}