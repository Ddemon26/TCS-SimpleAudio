# TCS Simple Audio Manager

![GitHub Forks](https://img.shields.io/github/forks/Ddemon26/TCS-AudioManager)
![GitHub Contributors](https://img.shields.io/github/contributors/Ddemon26/TCS-AudioManager)
![GitHub Stars](https://img.shields.io/github/stars/Ddemon26/TCS-AudioManager)
![GitHub Repo Size](https://img.shields.io/github/repo-size/Ddemon26/TCS-AudioManager)

[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Us-7289DA?logo=discord&logoColor=white)](https://discord.gg/knwtcq3N2a)
![Discord](https://img.shields.io/discord/1047781241010794506)

A lightweight audio management system for Unity that provides an easy way to control audio volumes, manage speaker modes, and handle seamless transitions between music tracks for both game and menu states.

## Overview

The TCS Simple Audio Manager is built for Unity projects to simplify the management of various audio channels. It allows you to:
- **Persist audio settings** using Unity’s PlayerPrefs.
- **Control multiple audio channels** (Master, Music, Menu Music, Game Sounds, and Voices) with dedicated volume controls.
- **Handle speaker mode changes** and ensure the mixer is updated accordingly.
- **Perform smooth audio transitions** such as crossfading between game and menu tracks.
- **Store and manage collections of audio clips** through a custom ScriptableObject.

## Project Structure

### AudioManagerPrefs.cs
- **Purpose:**  
  Wraps Unity’s PlayerPrefs system to save and load local audio settings such as volume levels and speaker mode.
- **Key Features:**
   - Manages keys for master volume, music volume, menu music volume, game sounds, and voices.
   - Provides getters and setters for each setting.

### AudioType.cs
- **Purpose:**  
  Defines an enumeration for different audio types.
- **Key Types:**
   - `Master`
   - `Music`
   - `MenuMusic`
   - `GameSounds`
   - `Voices`

### AudioVolumes.cs
- **Purpose:**  
  Provides a high-level interface for managing audio volumes and speaker modes.
- **Key Features:**
   - Retrieves initial volume levels from `AudioManagerPrefs.cs`.
   - Updates mixer groups via `MixerGroups.cs` based on slider values.
   - Includes methods to save current settings and reset volumes either by type or to default values.

### MixerGroups.cs
- **Purpose:**  
  Acts as a bridge between slider values (0 to 1) and the AudioMixer’s decibel settings.
- **Key Features:**
   - Converts linear volume values to decibels.
   - Maps each `AudioType` to its corresponding mixer parameter.
   - Provides methods to set, get, and clear mixer group parameters.

### SimpleAudioManager.cs
- **Purpose:**  
  Manages overall audio playback, including track transitions and crossfading between game and menu audio sources.
- **Key Features:**
   - Initializes audio sources for game and menu music.
   - Implements automatic track transitions based on a transition threshold.
   - Supports crossfade transitions when toggling between menu and game states.
   - Exposes a public property (`Volumes`) for runtime volume adjustments.

### SoundClips.cs
- **Purpose:**  
  A ScriptableObject that stores collections of audio clips.
- **Key Features:**
   - Stores a collection of `AudioClip` references.
   - Provides methods to retrieve a clip or its length safely, ensuring index validity.

## Installation and Setup

1. **Import Files:**  
   Add all provided scripts into your Unity project. Ensure that the namespaces (`TCS.SimpleAudio` and `TCS.AudioManager`) are maintained.

2. **Audio Mixer Setup:**
   - Create or import an AudioMixer.
   - Set up the exposed parameters corresponding to the keys used in `MixerGroups.cs` (e.g., `MasterVolume`, `MusicVolume`, etc.).

3. **Scriptable Object for Audio Clips:**
   - In Unity, right-click in the Project window and select **Create > Tent City Studio > Audio > SoundClipsCollection**.
   - Populate the resulting asset with your desired audio clips.

4. **Attach the SimpleAudioManager:**
   - Drag the `SimpleAudioManager` script onto a GameObject in your scene.
   - Assign the Audio Mixer, AudioSources (for music and menu music), and SoundClips assets in the Inspector.
   - Configure fade durations and track transition thresholds as needed.

5. **Runtime Adjustments:**
   - Use the exposed `Volumes` property of `SimpleAudioManager` to adjust volume levels in your game logic or UI controls.

## Usage

- **Changing Audio Settings:**  
  The audio volumes are automatically loaded on start via `AudioManagerPrefs.cs`. Changes made during runtime are saved on application quit (for non-editor builds) through the `OnApplicationQuit` method in `SimpleAudioManager.cs`.

- **Dynamic Audio Transitions:**  
  Toggle the `IsMenuOpen` property to trigger crossfade transitions between game and menu music. Automatic track transitions occur when the remaining time on a track falls below the defined threshold.

- **Volume Control:**  
  The `AudioVolumes` class clamps volume values between 0 and 1 and applies the correct decibel conversion for the AudioMixer.

## Contributing

If you have suggestions or improvements, feel free to fork the repository and create a pull request. Contributions are welcome!

## License

*Include license details here if applicable.*


## Support

Join our community on Discord for support, feedback, and discussions:  
[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Us-7289DA?logo=discord&logoColor=white)](https://discord.gg/knwtcq3N2a)

