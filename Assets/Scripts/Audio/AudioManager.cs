using Game.Common;
using UnityEngine;

namespace Game.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource sfxSourceLowVolume;

        [SerializeField] private float defaultMusicVolume = 0.7f;


        protected override void Awake()
        {
            base.Awake();
            SetMusicVolume(defaultMusicVolume);
        }

        public void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip, bool randomizePitch = false)
        {
            if (randomizePitch)
            {
                sfxSource.pitch = Random.Range(0.8f, 1.2f);
            }
            else
            {
                sfxSource.pitch = 1f;
            }
            sfxSource.PlayOneShot(clip);
        }

        public void PlayLowVolumeSFX(AudioClip clip, bool randomizePitch = false)
        {            
            if (randomizePitch)
            {
                sfxSourceLowVolume.pitch = Random.Range(0.8f, 1.2f);
            }
            else
            {
                sfxSourceLowVolume.pitch = 1f;
            }
            sfxSourceLowVolume.PlayOneShot(clip);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void ResetMusicVolume()
        {
            SetMusicVolume(defaultMusicVolume);
        }
    }

}