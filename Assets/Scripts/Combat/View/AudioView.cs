using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 音频视图 - 管理实体的音效播放
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioView : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _healSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _perfectBlockSound;
        [SerializeField] private AudioClip _counterSound;
        [SerializeField] private AudioClip _dodgeSound;
        [SerializeField] private AudioClip _attackSound;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void PlayHitSound()
        {
            PlayClip(_hitSound);
        }

        public void PlayHealSound()
        {
            PlayClip(_healSound);
        }

        public void PlayDeathSound()
        {
            PlayClip(_deathSound);
        }

        public void PlayPerfectBlockSound()
        {
            PlayClip(_perfectBlockSound);
        }

        public void PlayCounterSound()
        {
            PlayClip(_counterSound);
        }

        public void PlayDodgeSound()
        {
            PlayClip(_dodgeSound);
        }

        public void PlayAttackSound()
        {
            PlayClip(_attackSound);
        }

        public void PlayCustomSound(AudioClip clip)
        {
            PlayClip(clip);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip, _volume);
        }
    }
}
