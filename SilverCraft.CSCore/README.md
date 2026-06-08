![cscore.svg](https://raw.githubusercontent.com/thesilvercraft/cscore/master/docfx/images/cscore.svg)

# SilverCraft's fork of CSCore
Located at https://github.com/thesilvercraft/cscore  
I've decided to fork CSCore to remove features i will not use, in order to use CSCore for simple playback.  
Many features are removed, or broken or may be removed or broken in a future update.   
Check out the [original cscore project](https://github.com/filoe/cscore)  
A short list of differences between this project and the original cscore:
- Linux support (OpenAL, PortAudio as addon)  
- Removed windows support (no directx decoding for MP3s, no mediafoundation apis,...)  
- Removed FFMpeg support  
- Packaged on nuget   
- Optional sndfile and openmpt wrappers