---
_layout: landing
---
![cscore.svg](images/cscore.svg)

# SilverCraft's fork of CSCore
Check out the [original cscore project](https://github.com/filoe/cscore)  
A short list of differences between this project and the original cscore:
- Linux support (OpenAL, PortAudio as addon)
- Removed windows support (no directx decoding for MP3s, no mediafoundation apis,...)
- Removed FFmpeg support
- Packaged on nuget
- Optional sndfile and openmpt wrappers

Nuget packages:
- https://www.nuget.org/packages/SilverCraft.CSCore
- https://www.nuget.org/packages/SilverCraft.CSCore.SndFile
- https://www.nuget.org/packages/SilverCraft.OpenMPT
- https://www.nuget.org/packages/SilverCraft.CSCore.PortAudio