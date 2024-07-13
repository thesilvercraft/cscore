namespace SilverCraft.CSCore.SoundOut.AL
{
    /// <summary>
    /// Defines different OpenAL formats.
    /// </summary>
    // https://github.com/opentk/opentk/blob/32cef366709a6edc0808e6792f9d0ebd291e99da/src/OpenAL/OpenTK.Audio.OpenAL/AL/ALEnums.cs#L220
    public enum ALFormat
    {
      /// <summary>1 Channel, 8 bits per sample.</summary>
      Mono8Bit = 0x1100,

        /// <summary>1 Channel, 16 bits per sample.</summary>
        Mono16Bit = 0x1101,

        /// <summary>2 Channels, 8 bits per sample each.</summary>
        Stereo8Bit = 0x1102,

        /// <summary>2 Channels, 16 bits per sample each.</summary>
        Stereo16Bit = 0x1103,

        /// <summary>1 Channel, A-law encoded data. Requires Extension: AL_EXT_ALAW</summary>
        MonoALaw = 0x10016,

        /// <summary>2 Channels, A-law encoded data. Requires Extension: AL_EXT_ALAW</summary>
        StereoALaw = 0x10017,

        /// <summary>1 Channel, µ-law encoded data. Requires Extension: AL_EXT_MULAW</summary>
        MonoMuLaw = 0x10014,

        /// <summary>2 Channels, µ-law encoded data. Requires Extension: AL_EXT_MULAW</summary>
        StereoMuLaw = 0x10015,

        /// <summary>Ogg Vorbis encoded data. Requires Extension: AL_EXT_vorbis</summary>
        Vorbis = 0x10003,

        /// <summary>MP3 encoded data. Requires Extension: AL_EXT_mp3</summary>
        Mp3 = 0x10020,

        /// <summary>1 Channel, IMA4 ADPCM encoded data. Requires Extension: AL_EXT_IMA4</summary>
        MonoIma4 = 0x1300,

        /// <summary>2 Channels, IMA4 ADPCM encoded data. Requires Extension: AL_EXT_IMA4</summary>
        StereoIma4 = 0x1301,

        /// <summary>1 Channel, single-precision floating-point data. Requires Extension: AL_EXT_float32</summary>
        MonoFloat32Bit = 0x10010,

        /// <summary>2 Channels, single-precision floating-point data. Requires Extension: AL_EXT_float32</summary>
        StereoFloat32Bit = 0x10011,

        /// <summary>1 Channel, double-precision floating-point data. Requires Extension: AL_EXT_double</summary>
        MonoDouble = 0x10012,

        /// <summary>2 Channels, double-precision floating-point data. Requires Extension: AL_EXT_double</summary>
        StereoDouble = 0x10013,

        /// <summary>Multichannel 5.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn16Bit = 0x120B,

        /// <summary>Multichannel 5.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn32Bit = 0x120C,

        /// <summary>Multichannel 5.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn8Bit = 0x120A,

        /// <summary>Multichannel 6.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn16Bit = 0x120E,

        /// <summary>Multichannel 6.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn32Bit = 0x120F,

        /// <summary>Multichannel 6.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn8Bit = 0x120D,

        /// <summary>Multichannel 7.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn16Bit = 0x1211,

        /// <summary>Multichannel 7.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn32Bit = 0x1212,

        /// <summary>Multichannel 7.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn8Bit = 0x1210,

        /// <summary>Multichannel 4.0, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad16Bit = 0x1205,

        /// <summary>Multichannel 4.0, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad32Bit = 0x1206,

        /// <summary>Multichannel 4.0, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad8Bit = 0x1204,

        /// <summary>1 Channel rear speaker, 16-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear16Bit = 0x1208,

        /// <summary>1 Channel rear speaker, 32-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear32Bit = 0x1209,

        /// <summary>1 Channel rear speaker, 8-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear8Bit = 0x1207,
    }
}
