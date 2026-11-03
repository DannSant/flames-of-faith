using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// A few AudioSources built at runtime from a template, so every sound gets its own source and its own pitch.
    /// With one shared source, setting the pitch for a new sound re-pitched every sound it was still playing.
    /// A new sound takes a free voice, or the one started longest ago.
    /// </summary>
    public class SfxVoicePool
    {
        private readonly AudioSource[] voices;
        private readonly float[] volumeScales;
        private readonly float[] startTimes;
        private float baseVolume = 1f;

        public SfxVoicePool(AudioSource template, int size, Transform parent)
        {
            size = Mathf.Max(1, size);
            voices = new AudioSource[size];
            volumeScales = new float[size];
            startTimes = new float[size];
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject($"SfxVoice{i}");
                go.transform.SetParent(parent, false);
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                if (template != null)
                {
                    source.outputAudioMixerGroup = template.outputAudioMixerGroup;
                    source.priority = template.priority;
                    source.spatialBlend = template.spatialBlend;
                    source.panStereo = template.panStereo;
                    source.bypassEffects = template.bypassEffects;
                    source.bypassListenerEffects = template.bypassListenerEffects;
                    source.bypassReverbZones = template.bypassReverbZones;
                    source.ignoreListenerPause = template.ignoreListenerPause;
                }
                voices[i] = source;
                startTimes[i] = float.NegativeInfinity;
            }
        }

        public int Count => voices.Length;

        /// <summary>The SFX volume (settings × master). Reaches every voice, including those playing.</summary>
        public void SetBaseVolume(float volume)
        {
            baseVolume = Mathf.Clamp01(volume);
            for (int i = 0; i < voices.Length; i++)
            {
                if (voices[i] != null) voices[i].volume = baseVolume * volumeScales[i];
            }
        }

        public void Play(AudioClip clip, float volumeScale, float pitch)
        {
            if (clip == null) return;

            int index = PickVoice();
            var source = voices[index];
            if (source == null) return;

            volumeScales[index] = Mathf.Clamp01(volumeScale);
            startTimes[index] = Time.unscaledTime;
            source.Stop();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = baseVolume * volumeScales[index];
            source.Play();
        }

        private int PickVoice()
        {
            int oldest = 0;
            for (int i = 0; i < voices.Length; i++)
            {
                if (voices[i] != null && !voices[i].isPlaying) return i;
                if (startTimes[i] < startTimes[oldest]) oldest = i;
            }
            return oldest;
        }
    }
}
