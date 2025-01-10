using UnityEngine;
namespace TCS.SimpleAudio {
    /// <summary>
    /// Singleton class which saves/loads local-client settings.
    /// (This is just a wrapper around the PlayerPrefs system,
    /// so that all the calls are in the same place.)
    /// </summary>
    public class AudioManagerPrefs {
        const string K_MASTER_VOLUME_KEY = "MasterVolume";
        const string K_MUSIC_VOLUME_KEY = "MusicVolume";
        const string K_MENU_MUSIC_VOLUME_KEY = "MenuMusicVolume";
        const string K_GAME_SOUNDS_VOLUME_KEY = "GameSoundsVolume";
        const string K_VOICE_VOLUME_KEY = "VoicesVolume";
        
        //const string K_CLIENT_GUID_KEY = "client_guid";

        const float K_DEFAULT_MASTER_VOLUME = 1f;
        const float K_DEFAULT_MUSIC_VOLUME = 0.5f;
        const float K_DEFAULT_MENU_MUSIC_VOLUME = 0.5f;
        const float K_DEFAULT_GAME_SOUNDS_VOLUME = 0.5f;
        const float K_DEFAULT_VOICE_VOLUME = 0.5f;

        public float GetMasterVolume() => PlayerPrefs.GetFloat(K_MASTER_VOLUME_KEY, K_DEFAULT_MASTER_VOLUME);
        public void SetMasterVolume(float volume) => PlayerPrefs.SetFloat(K_MASTER_VOLUME_KEY, volume);
        public float GetMusicVolume() => PlayerPrefs.GetFloat(K_MUSIC_VOLUME_KEY, K_DEFAULT_MUSIC_VOLUME);
        public void SetMusicVolume(float volume) => PlayerPrefs.SetFloat(K_MUSIC_VOLUME_KEY, volume);
        public float GetMenuMusicVolume() => PlayerPrefs.GetFloat(K_MENU_MUSIC_VOLUME_KEY, K_DEFAULT_MENU_MUSIC_VOLUME);
        public void SetMenuMusicVolume(float volume) => PlayerPrefs.SetFloat(K_MENU_MUSIC_VOLUME_KEY, volume);
        public float GetGameSoundsVolume() => PlayerPrefs.GetFloat(K_GAME_SOUNDS_VOLUME_KEY, K_DEFAULT_GAME_SOUNDS_VOLUME);
        public void SetGameSoundsVolume(float volume) => PlayerPrefs.SetFloat(K_GAME_SOUNDS_VOLUME_KEY, volume);
        public float GetVoiceVolume() => PlayerPrefs.GetFloat(K_VOICE_VOLUME_KEY, K_DEFAULT_VOICE_VOLUME);
        public void SetVoiceVolume(float volume) => PlayerPrefs.SetFloat(K_VOICE_VOLUME_KEY, volume);

        /*/// <summary>
        /// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
        /// </summary>
        /// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
        public string GetGuid() {
            if (PlayerPrefs.HasKey(K_CLIENT_GUID_KEY)) {
                return PlayerPrefs.GetString(K_CLIENT_GUID_KEY);
            }

            var guid = System.Guid.NewGuid();
            var guidString = guid.ToString();

            PlayerPrefs.SetString(K_CLIENT_GUID_KEY, guidString);
            return guidString;
        }*/
    }
}