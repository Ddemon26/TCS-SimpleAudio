using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace TCS.SimpleAudio {
    public class MixerGroups {
        const string MASTER = "MasterVolume";
        const string MUSIC = "MusicVolume";
        const string MENU_MUSIC = "MenuMusicVolume";
        const string GAME_SOUNDS = "GameSoundsVolume";
        const string VOICES = "VoicesVolume";

        readonly AudioMixer m_mixer;

        /// <summary>
        /// The audio sliders use a value between 0.0001 and 1, but the mixer works in decibels -- by default, -80 to 0.
        /// To convert, we use log10(slider) multiplied by 20. Why 20? because log10(.0001)*20=-80, which is the
        /// bottom range for our mixer, meaning it's disabled.
        /// </summary>
        const float K_VOLUME_LOG10_MULTIPLIER = 20;

        public MixerGroups(AudioMixer mixer) {
            m_mixer = mixer;
        }
        
        // New Version on handling the audio mixer
        readonly Dictionary<AudioType, string> m_audioTypeToString = new() {
            { AudioType.Master, MASTER },
            { AudioType.Music, MUSIC },
            { AudioType.MenuMusic, MENU_MUSIC },
            { AudioType.GameSounds, GAME_SOUNDS },
            { AudioType.Voices, VOICES }
        };

        public void SetFloatByType(AudioType type, float value) {
            if (m_audioTypeToString.TryGetValue(type, out string parameterName)) {
                m_mixer.SetFloat(parameterName, GetVolumeInDecibels(value));
            } else {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public float GetFloatByType(AudioType type) {
            if (m_audioTypeToString.TryGetValue(type, out string parameterName)) {
                if (m_mixer.GetFloat(parameterName, out float value)) {
                    return Mathf.Pow(10, value / 20);
                }
            } else {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return 0f; // Default return value in case of failure
        }

        public void ClearFloatsByType(AudioType type) {
            if (m_audioTypeToString.TryGetValue(type, out string parameterName)) {
                m_mixer.ClearFloat(parameterName);
            } else {
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        static float GetVolumeInDecibels(float volume) {
            if (volume <= 0) // sanity-check in case we have bad prefs data
            {
                volume = 0.0001f;
            }

            return Mathf.Log10(volume) * K_VOLUME_LOG10_MULTIPLIER;
        }
    }
}