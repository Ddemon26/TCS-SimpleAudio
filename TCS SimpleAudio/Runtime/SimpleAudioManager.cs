using System;
using System.Collections;
using System.Collections.Generic;
using TCS.AudioManager;
using UnityEngine;
using UnityEngine.Audio;
namespace TCS.SimpleAudio {
    [DefaultExecutionOrder(-100)]
    public class SimpleAudioManager : MonoBehaviour {
        /*#region Simple Singleton
        static SimpleAudioManager s_instance;
        void InitializeSingleton() {
            if (!Application.isPlaying) return;

            // Detach from parent
            transform.SetParent(null);

            if (!s_instance) {
                s_instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                if (s_instance != this) {
                    Destroy(gameObject);
                }
            }
        }
        #endregion*/

        [Header("Audio Mixer Reference")]
        [SerializeField, HideInInspector] AudioMixer m_mixer;

        [Header("Music Clips")]
        [SerializeField] SoundClips m_musicClips;
        [SerializeField] SoundClips m_menuMusicClips;

        [Header("Audio Sources")]
        [SerializeField] AudioSource m_musicSource;
        [SerializeField] AudioSource m_menuMusicSource;

        [Header("Fade Durations")]
        [SerializeField] float m_fadeInTime = 2.0f; // Duration for fading in
        [SerializeField] float m_fadeOutTime = 2.0f; // Duration for fading out

        [Header("Automatic Track Transition")]
        [Tooltip("If a track has <= this amount of time left, the manager will begin switching to the next track.")]
        [SerializeField] float m_trackTransitionThreshold = 2.0f;

        // Track indices for game music and menu music
        int m_musicClipIndex;
        int m_menuMusicClipIndex;

        // Flags to prevent overlapping transitions
        bool m_isSwitchingMusicTrack;
        bool m_isSwitchingMenuTrack;
        bool m_isFadingBetweenSources;

        // Keep track of the previous menu state
        bool m_previousIsMenuOpen;

        // External property to toggle between menu music and game music
        public bool IsMenuOpen { get; set; }
        
        //public void ToggleMenuMusic() => IsMenuOpen = !IsMenuOpen;

        // Gives access to volume controls if needed
        public AudioVolumes Volumes { get; private set; }

        void Awake() {
            Volumes = new AudioVolumes(m_mixer);
            //InitializeSingleton();

            // Initialize track indices
            m_musicClipIndex = 0;
            m_menuMusicClipIndex = UnityEngine.Random.Range(0, m_menuMusicClips.m_clips.Length);
            
            IsMenuOpen = true;
            m_musicSource.volume = 0f;
            m_menuMusicSource.volume = 0f;
            // Assign initial clips
            SetMusicSourceClip(m_musicClips, m_musicSource, m_musicClipIndex);
            SetMusicSourceClip(m_menuMusicClips, m_menuMusicSource, m_menuMusicClipIndex);
        }

        void Update() {
            // Handle automatic track transitions for both sources
            HandleMenuTrackTransition();
            HandleGameTrackTransition();

            // If the menu state changed since last frame, fade between music sources
            if (m_previousIsMenuOpen != IsMenuOpen && !m_isFadingBetweenSources) {
                StartCoroutine(FadeBetweenMusicSources(IsMenuOpen));
                m_previousIsMenuOpen = IsMenuOpen;
            }
        }

        #region Handle Automatic Track Transitions
        void HandleGameTrackTransition() {
            // If user is not in the menu, the musicSource should handle track transitions
            if (!IsMenuOpen) {
                // Start playing if idle and not switching
                if (!m_musicSource.isPlaying && !m_isSwitchingMusicTrack) {
                    m_musicSource.Play();
                }

                // Check if track is near the end and not already switching
                if (m_musicSource.isPlaying &&
                    (m_musicSource.clip.length - m_musicSource.time) <= m_trackTransitionThreshold &&
                    !m_isSwitchingMusicTrack) {
                    StartCoroutine(FadeOutAndSwitchMusic());
                }
            }
        }

        void HandleMenuTrackTransition() {
            // If user is in the menu, the menuMusicSource should handle track transitions
            if (IsMenuOpen) {
                // Start playing if idle and not switching
                if (!m_menuMusicSource.isPlaying && !m_isSwitchingMenuTrack) {
                    m_menuMusicSource.Play();
                }

                // Check if track is near the end and not already switching
                if (m_menuMusicSource.isPlaying &&
                    (m_menuMusicSource.clip.length - m_menuMusicSource.time) <= m_trackTransitionThreshold &&
                    !m_isSwitchingMenuTrack) {
                    StartCoroutine(FadeOutAndSwitchMenu());
                }
            }
        }
        #endregion

        #region Fade Out and Switch Track (Game / Menu)
        IEnumerator FadeOutAndSwitchMusic() {
            m_isSwitchingMusicTrack = true;

            // 1) Fade out current music track
            yield return FadeOut(m_musicSource, m_fadeOutTime);

            // 2) Move to the next track
            m_musicClipIndex = (m_musicClipIndex + 1) % m_musicClips.m_clips.Length;
            SetMusicSourceClip(m_musicClips, m_musicSource, m_musicClipIndex);

            // 3) Fade in the new track
            yield return FadeIn(m_musicSource, m_fadeInTime);

            m_isSwitchingMusicTrack = false;
        }

        IEnumerator FadeOutAndSwitchMenu() {
            m_isSwitchingMenuTrack = true;

            // 1) Fade out current menu track
            yield return FadeOut(m_menuMusicSource, m_fadeOutTime);

            // 2) Move to the next track
            m_menuMusicClipIndex = (m_menuMusicClipIndex + 1) % m_menuMusicClips.m_clips.Length;
            SetMusicSourceClip(m_menuMusicClips, m_menuMusicSource, m_menuMusicClipIndex);

            // 3) Fade in the new track
            yield return FadeIn(m_menuMusicSource, m_fadeInTime);

            m_isSwitchingMenuTrack = false;
        }
        #endregion

        #region Fade Between Sources (Game <-> Menu)
        IEnumerator FadeBetweenMusicSources(bool fadeToMenu) {
            m_isFadingBetweenSources = true;

            // Decide which source is fading out and which is fading in
            var fadingOutSource = fadeToMenu ? m_musicSource : m_menuMusicSource;
            var fadingInSource = fadeToMenu ? m_menuMusicSource : m_musicSource;

            // Fade out the current active source
            yield return FadeOut(fadingOutSource, m_fadeOutTime);

            // Fade in the other source
            yield return FadeIn(fadingInSource, m_fadeInTime);

            m_isFadingBetweenSources = false;
        }
        #endregion

        #region Fade Methods
        static IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0f) {
                audioSource.volume -= startVolume * (Time.deltaTime / fadeTime);
                yield return null;
            }

            audioSource.Stop();
            // Reset volume so next FadeIn starts from 0 up to startVolume
            audioSource.volume = startVolume;
        }

        static IEnumerator FadeIn(AudioSource audioSource, float fadeTime) {
            const float targetVolume = 1.0f; // or make this configurable if desired
            audioSource.volume = 0f;
            audioSource.Play();

            while (audioSource.volume < targetVolume) {
                audioSource.volume += targetVolume * (Time.deltaTime / fadeTime);
                yield return null;
            }
        }
        #endregion

        #region Utility
        void SetMusicSourceClip(SoundClips clips, AudioSource source, int index) {
            source.clip = clips.GetClip(index);
        }
        #endregion

        void OnApplicationQuit() {
            if (!Application.isEditor) {
                Volumes.SaveAll();
            }
        }
    }

    public enum AudioType {
        Master = 0,
        Music = 1,
        MenuMusic = 2,
        GameSounds = 3,
        Voices = 4,
    }

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

        // Old Version on handling the audio mixer
        /*public void SetFloatByType(AudioType type, float value) {
            switch (type) {
                case AudioType.Master:
                    m_mixer.SetFloat(MASTER, GetVolumeInDecibels(value));
                    break;
                case AudioType.Music:
                    m_mixer.SetFloat(MUSIC, GetVolumeInDecibels(value));
                    break;
                case AudioType.MenuMusic:
                    m_mixer.SetFloat(MENU_MUSIC, GetVolumeInDecibels(value));
                    break;
                case AudioType.GameSounds:
                    m_mixer.SetFloat(GAME_SOUNDS, GetVolumeInDecibels(value));
                    break;
                case AudioType.Voices:
                    m_mixer.SetFloat(VOICES, GetVolumeInDecibels(value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public float GetFloatByType(AudioType type) {
            float value;
            switch (type) {
                case AudioType.Master:
                    m_mixer.GetFloat(MASTER, out value);
                    break;
                case AudioType.Music:
                    m_mixer.GetFloat(MUSIC, out value);
                    break;
                case AudioType.MenuMusic:
                    m_mixer.GetFloat(MENU_MUSIC, out value);
                    break;
                case AudioType.GameSounds:
                    m_mixer.GetFloat(GAME_SOUNDS, out value);
                    break;
                case AudioType.Voices:
                    m_mixer.GetFloat(VOICES, out value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            value = Mathf.Pow(10, value / 20);

            return value;
        }

        public void ClearFloatsByType(AudioType type) {
            switch (type) {
                case AudioType.Master:
                    m_mixer.ClearFloat(MASTER);
                    break;
                case AudioType.GameSounds:
                    m_mixer.ClearFloat(GAME_SOUNDS);
                    break;
                case AudioType.Music:
                    m_mixer.ClearFloat(MUSIC);
                    break;
                case AudioType.MenuMusic:
                    m_mixer.ClearFloat(MENU_MUSIC);
                    break;
                case AudioType.Voices:
                    m_mixer.ClearFloat(VOICES);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        static float GetVolumeInDecibels(float volume) {
            if (volume <= 0) // sanity-check in case we have bad prefs data
            {
                volume = 0.0001f;
            }

            return Mathf.Log10(volume) * K_VOLUME_LOG10_MULTIPLIER;
        }*/
        
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