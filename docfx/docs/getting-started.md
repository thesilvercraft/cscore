# Getting Started with CSCore

This guide will walk you through creating a simple .NET console application that plays a FLAC audio file using the `SilverCraft.CSCore` library.

---

## Prerequisites

Before you begin, ensure you have the following installed on your system:
* **.NET SDK** (8.0 or newer recommended)
* **OpenAL** (Required for audio playback)
* **wget** or **curl** (For downloading the sample file)
* **unzip** (To extract the sample file)

---

## Step 1: Set Up Your Project

Open your terminal and run the following commands to create a new directory and initialize a .NET console application.

```bash
# Create and navigate to the project directory
mkdir cscoretestapp
cd cscoretestapp

# Create a new .NET console project
dotnet new console

# Add the SilverCraft.CSCore NuGet package
dotnet add package SilverCraft.CSCore

```

---

## Step 2: Download Sample Audio

Download and extract a sample FLAC file to use for testing.

```bash
# Download the sample ZIP file
wget https://helpguide.sony.net/high-res/sample1/v1/data/Sample_BeeMoved_96kHz24bit.flac.zip

# Extract the FLAC file from the archive
unzip Sample_BeeMoved_96kHz24bit.flac.zip

# Clean up the ZIP file
rm Sample_BeeMoved_96kHz24bit.flac.zip

```


---

## Step 3: Implement the Code

Replace the entire contents of your `Program.cs` file with the following code:

```csharp
using SilverCraft.CSCore;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.SoundOut;

// Load the downloaded FLAC file
FlacFile soundFile = new("Sample_BeeMoved_96kHz24bit.flac");

try
{
    // Initialize the OpenAL audio output device
    using var outputDevice = new ALSoundOut();
    
    outputDevice.Initialize(soundFile);
    outputDevice.Volume = 0.3f; // Set volume (0.0 to 1.0)
    
    Console.WriteLine("Playing audio... Press Ctrl+C to stop.");
    outputDevice.Play();
    
    // Block the thread until the audio finishes playing
    outputDevice.WaitForStopped();
}
finally
{
    // Ensure the file stream is properly closed and released
    soundFile.Dispose();
}

```

---

## Step 4: Run the Application

Execute the application using the .NET CLI:

```bash
dotnet run

```

---

## Troubleshooting

### OpenAL Errors

If you encounter errors related to OpenAL driver initialization, the OpenAL runtime library might be missing from your system.

* **Windows:** Download and run the OpenAL Installer from the official OpenAL website.
* **Linux (Ubuntu/Debian):** Install it via your package manager: `sudo apt-get install libopenal1`

### Sample File Download Fails

If the Sony sample link is broken or unavailable:

1. Download any standard `.flac` audio file manually.
2. Place it directly into your `cscoretestapp` project directory.
3. Update the filename in `Program.cs` to match your downloaded file:
```csharp
FlacFile soundFile = new("your_audio_file.flac");
```

