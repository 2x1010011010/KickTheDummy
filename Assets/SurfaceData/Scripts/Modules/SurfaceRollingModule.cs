using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	public class SurfaceRollingModule : SurfaceModule
	{
		[SerializeField] private AudioClip m_clip;

		[Space]
		[SerializeField] private float m_forceMultiplier = 1;


		public bool PlaySound( ContinuousData data, Dictionary<ContinuousData, AudioSourcePoolable> continuousAudioSources, float volumeMultiplier = 1 )
		{
			float force = data.Force;
			force *= m_forceMultiplier;

			float volume = force * volumeMultiplier;
			volume = Mathf.Clamp01( volume );

			float pitch = 0.5f + volume * 0.7f;

			if( !continuousAudioSources.TryGetValue( data, out AudioSourcePoolable audioSource ) )
			{
				if( volume < 0.01f )
					return false;

				audioSource = AudioSourcesPool.GetAudioSource();
				audioSource.position = data.WorldPosition;
				audioSource.clip = m_clip;
				audioSource.loop = true;
				audioSource.smoothVolume = true;
				audioSource.volume = volume;
				audioSource.playtime = Random.Range( 0, 1f );
				audioSource.pitch = pitch;
				audioSource.Play();

				continuousAudioSources.Add( data, audioSource );
			}
			else
			{
				audioSource.position = data.WorldPosition;
				audioSource.volume = volume;
				audioSource.pitch = pitch;

				if( volume < 0.01f )
				{
					audioSource.FadeAway( 0f );
					continuousAudioSources.Remove( data );
				}
			}
			return true;
		}
	}
}