using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurfaceDataSystem
{
    public class VisualEffectModule : MonoBehaviour
    {
        [SerializeField] private AudioClip m_slide;

        [Space]
        [SerializeField] private float m_forceMultiplier = 1;

        public bool PlayVisualEffect(ContinuousData data, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float volumeMultiplier = 1)
        {
            float force = data.Force;
            force *= m_forceMultiplier;

            float volume = force * volumeMultiplier;


            if (!continuousAudioSources.TryGetValue(data, out AudioSourcePoolable audioSource))
            {
                if (volume < 0.01f)
                    return false;

                audioSource = AudioSourcesPool.GetAudioSource();
                audioSource.position = data.WorldPosition;
                audioSource.clip = m_slide;
                audioSource.loop = true;
                audioSource.smoothVolume = true;
                audioSource.volume = volume;
                audioSource.playtime = Random.Range(0, 1f);
                audioSource.Play();

                continuousAudioSources.Add(data, audioSource);
            }
            else
            {
                audioSource.position = data.WorldPosition;
                audioSource.volume = volume;

                if (volume < 0.01f)
                {
                    audioSource.FadeAway(0f);
                    continuousAudioSources.Remove(data);
                }
            }
            return true;
        }
    }
}
