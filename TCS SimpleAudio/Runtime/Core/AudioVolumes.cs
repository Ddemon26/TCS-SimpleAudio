using UnityEngine;
using UnityEngine.Audio;
namespace TCS.SimpleAudio {
    public class AudioVolumes {
        readonly MixerGroups m_mixerGroups;
        readonly AudioManagerPrefs m_prefs;
        AudioConfiguration m_settings;
        AudioSpeakerMode m_speakerMode;

        public AudioVolumes(AudioMixer mixer) {
            m_mixerGroups = new MixerGroups(mixer);
            m_prefs = new AudioManagerPrefs();

            Master = m_prefs.GetMasterVolume();
            Music = m_prefs.GetMusicVolume();
            MenuMusic = m_prefs.GetMenuMusicVolume();
            GameSounds = m_prefs.GetGameSoundsVolume();
            Voices = m_prefs.GetVoiceVolume();
            
            SetSpeakerMode((int)m_prefs.GetSpeakerMode());

            m_settings = AudioSettings.GetConfiguration();
        }

        // We choose to use int instead of passing enums, is because we are not locking ourselves to a fixed enum type.
        // So in theory, we can pass any int value, and it will be converted to the correct enum type.
        public void SetSpeakerMode(int mode) {
            var speakerMode = mode switch {
                0 => AudioSpeakerMode.Stereo,
                1 => AudioSpeakerMode.Mono,
                2 => AudioSpeakerMode.Stereo,
                3 => AudioSpeakerMode.Quad,
                4 => AudioSpeakerMode.Surround,
                5 => AudioSpeakerMode.Mode5point1,
                6 => AudioSpeakerMode.Mode7point1,
                7 => AudioSpeakerMode.Stereo, // for some reason 7 (Prologic) throws an error, find out why sometime.
                _ => AudioSpeakerMode.Stereo,
            };

            var config = AudioSettings.GetConfiguration();

            // Check if the new speaker mode is different from the current one
            if (config.speakerMode != speakerMode) {
                config.speakerMode = speakerMode;
                AudioSettings.Reset(config);
                //Debug.Log($"Set speaker mode to {config.speakerMode}");
            }
        }

        
        public AudioSpeakerMode GetSpeakerMode() => m_settings.speakerMode;

        //we use get and setters to ensure that the values are always within the 0-1 range
        public float Master {
            get => m_mixerGroups.GetFloatByType(AudioType.Master);
            set => m_mixerGroups.SetFloatByType(AudioType.Master, Mathf.Clamp(value, 0f, 1f));
        }

        public float Music {
            get => m_mixerGroups.GetFloatByType(AudioType.Music);
            set => m_mixerGroups.SetFloatByType(AudioType.Music, Mathf.Clamp(value, 0f, 1f));
        }

        public float MenuMusic {
            get => m_mixerGroups.GetFloatByType(AudioType.MenuMusic);
            set => m_mixerGroups.SetFloatByType(AudioType.MenuMusic, Mathf.Clamp(value, 0f, 1f));
        }

        public float GameSounds {
            get => m_mixerGroups.GetFloatByType(AudioType.GameSounds);
            set => m_mixerGroups.SetFloatByType(AudioType.GameSounds, Mathf.Clamp(value, 0f, 1f));
        }

        public float Voices {
            get => m_mixerGroups.GetFloatByType(AudioType.Voices);
            set => m_mixerGroups.SetFloatByType(AudioType.Voices, Mathf.Clamp(value, 0f, 1f));
        }

        public void SaveAll() {
            m_prefs.SetMasterVolume(Master);
            m_prefs.SetMusicVolume(Music);
            m_prefs.SetMenuMusicVolume(MenuMusic);
            m_prefs.SetGameSoundsVolume(GameSounds);
            m_prefs.SetVoiceVolume(Voices);
            
            m_prefs.SetSpeakerMode(GetSpeakerMode());
        }

        public void ResetByType(AudioType type) => m_mixerGroups.ClearFloatsByType(type);

        public void ResetToDefault() {
            m_mixerGroups.ClearFloatsByType(AudioType.Master);
            m_mixerGroups.ClearFloatsByType(AudioType.Music);
            m_mixerGroups.ClearFloatsByType(AudioType.MenuMusic);
            m_mixerGroups.ClearFloatsByType(AudioType.GameSounds);
            m_mixerGroups.ClearFloatsByType(AudioType.Voices);
        }
    }
}