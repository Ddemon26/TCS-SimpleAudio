using System.Collections;
using TCS.AudioManager;
using UnityEngine;
using UnityEngine.Audio;
namespace TCS.SimpleAudio {
    [DefaultExecutionOrder(-100)]
    public class SimpleAudioManager : MonoBehaviour {
        [Header("Audio Mixer Reference")]
        [SerializeField] [HideInInspector] AudioMixer m_mixer;

        [Header("Music Clips")]
        [SerializeField] SoundClips m_musicClips;
        [SerializeField] SoundClips m_menuMusicClips;

        [Header("Audio Sources")]
        [SerializeField] AudioSource m_musicSource;
        [SerializeField] AudioSource m_menuMusicSource;
        [SerializeField] AudioListener m_mainMenuAudioListener;

        [Header("Fade Durations")]
        [SerializeField] float m_fadeInTime = 2.0f; // Duration for fading in
        [SerializeField] float m_fadeOutTime = 2.0f; // Duration for fading out

        [Header("Automatic Track Transition")]
        [Tooltip("If a track has <= this amount of time left, the manager will begin switching to the next track.")]
        [SerializeField] float m_trackTransitionThreshold = 2.0f;

        // Track indices
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

        // Provides access to volume controls if needed
        public AudioVolumes Volumes { get; private set; }

        void Awake() {
            //InitializeSingleton();
            Volumes = new AudioVolumes(m_mixer);

            // Initialize track indices
            m_musicClipIndex = 0;
            m_menuMusicClipIndex = Random.Range(0, m_menuMusicClips.m_clips.Length);

            // Assume menu is active initially
            IsMenuOpen = true;
            m_previousIsMenuOpen = IsMenuOpen;

            // Set initial volumes: the active source (menu) starts at full volume,
            // and the inactive one (game) starts muted.
            m_musicSource.volume = 0f;
            m_menuMusicSource.volume = 1f;

            // Assign the initial clips
            SetMusicSourceClip(m_musicClips, m_musicSource, m_musicClipIndex);
            SetMusicSourceClip(m_menuMusicClips, m_menuMusicSource, m_menuMusicClipIndex);

            // (Optional) Set looping if desired. Note: if you’re auto transitioning tracks,
            // you may want loop = false so that the transition code kicks in.
            m_musicSource.loop = false;
            m_menuMusicSource.loop = false;

            // Start both sources so they’re always playing.
            m_musicSource.Play();
            m_menuMusicSource.Play();
        }

        void Update() {
            // TODO: Refactor into a message system or event system to handle menu state changes.
            // Handle automatic track transitions for both sources
            HandleMenuTrackTransition();
            HandleGameTrackTransition();

            // If the menu state changed since the last frame, crossfade between sources.
            if (m_previousIsMenuOpen != IsMenuOpen && !m_isFadingBetweenSources) {
                StartCoroutine(FadeBetweenMusicSources(IsMenuOpen));
                m_previousIsMenuOpen = IsMenuOpen;
            }
        }
        
        public void ToggleAudioListener(bool isEnabled) {
            if (m_mainMenuAudioListener != null) {
                m_mainMenuAudioListener.enabled = isEnabled;
            }
        }

        #region Automatic Track Transitions
        void HandleGameTrackTransition() {
            if (!IsMenuOpen && m_musicSource.clip != null && !m_isSwitchingMusicTrack) {
                if (m_musicSource.clip.length - m_musicSource.time <= m_trackTransitionThreshold) {
                    StartCoroutine(FadeOutAndSwitchMusic());
                }
            }
        }

        void HandleMenuTrackTransition() {
            if (IsMenuOpen && m_menuMusicSource.clip != null && !m_isSwitchingMenuTrack) {
                if (m_menuMusicSource.clip.length - m_menuMusicSource.time <= m_trackTransitionThreshold) {
                    StartCoroutine(FadeOutAndSwitchMenu());
                }
            }
        }

        // For the active channel, fade out, switch to the next clip, and fade back in.
        IEnumerator FadeOutAndSwitchMusic() {
            m_isSwitchingMusicTrack = true;
            yield return StartCoroutine(CrossFadeOut(m_musicSource, m_fadeOutTime));
            m_musicClipIndex = (m_musicClipIndex + 1) % m_musicClips.m_clips.Length;
            SetMusicSourceClip(m_musicClips, m_musicSource, m_musicClipIndex);
            m_musicSource.Play(); // Begin the new clip
            yield return StartCoroutine(CrossFadeIn(m_musicSource, m_fadeInTime));
            m_isSwitchingMusicTrack = false;
        }

        IEnumerator FadeOutAndSwitchMenu() {
            m_isSwitchingMenuTrack = true;
            yield return StartCoroutine(CrossFadeOut(m_menuMusicSource, m_fadeOutTime));
            m_menuMusicClipIndex = (m_menuMusicClipIndex + 1) % m_menuMusicClips.m_clips.Length;
            SetMusicSourceClip(m_menuMusicClips, m_menuMusicSource, m_menuMusicClipIndex);
            m_menuMusicSource.Play(); // Begin the new clip
            yield return StartCoroutine(CrossFadeIn(m_menuMusicSource, m_fadeInTime));
            m_isSwitchingMenuTrack = false;
        }
        #endregion

        #region Crossfade Between Game and Menu Music
        // When switching between game and menu, we simply fade one out and the other in,
        // but we never stop either AudioSource.
        IEnumerator FadeBetweenMusicSources(bool fadeToMenu) {
            m_isFadingBetweenSources = true;

            // Decide which source to fade out and which to fade in.
            var fadingOutSource = fadeToMenu ? m_musicSource : m_menuMusicSource;
            var fadingInSource = fadeToMenu ? m_menuMusicSource : m_musicSource;

            yield return StartCoroutine(CrossFadeOut(fadingOutSource, m_fadeOutTime));
            yield return StartCoroutine(CrossFadeIn(fadingInSource, m_fadeInTime));

            m_isFadingBetweenSources = false;
        }
        #endregion

        #region Crossfade Methods (No Stop/Play)
        static IEnumerator CrossFadeOut(AudioSource audioSource, float fadeTime) {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0f) {
                audioSource.volume -= startVolume * (Time.deltaTime / fadeTime);
                yield return null;
            }

            audioSource.volume = 0f;
        }

        static IEnumerator CrossFadeIn(AudioSource audioSource, float fadeTime) {
            const float targetVolume = 1.0f;
            while (audioSource.volume < targetVolume) {
                audioSource.volume += targetVolume * (Time.deltaTime / fadeTime);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }
        #endregion

        #region Utility
        static void SetMusicSourceClip(SoundClips clips, AudioSource source, int index) {
            source.clip = clips.GetClip(index);
        }
        #endregion

        void OnApplicationQuit() {
            if (!Application.isEditor) {
                Volumes.SaveAll();
            }
        }
    }
}