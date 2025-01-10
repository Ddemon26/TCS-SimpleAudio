using UnityEngine;
namespace TCS.AudioManager {
    [CreateAssetMenu
    (
        menuName = "Tent City Studio/Audio/SoundClipsCollection",
        fileName = "SoundClipsCollection",
        order = 0
    )] public class SoundClips : ScriptableObject {
        public string m_name;
        public AudioClip[] m_clips;
        
        public AudioClip GetClip(int index) {
            if (index < 0 || index >= m_clips.Length) {
                //Debug.LogError($"Index {index} out of range for SoundClips {m_name}");
                return null;
            }
            return m_clips[index];
        }
        
        public float GetClipLength(int index) {
            if (index < 0 || index >= m_clips.Length) {
                //Debug.LogError($"Index {index} out of range for SoundClips {m_name}");
                return 0.0f;
            }
            return m_clips[index].length;
        }
    }
}