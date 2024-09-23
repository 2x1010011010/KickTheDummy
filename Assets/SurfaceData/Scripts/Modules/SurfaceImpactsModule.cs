using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	public class SurfaceImpactsModule : SurfaceModule
	{
		[SerializeField] private AudioClip[] m_lightClips;
		[SerializeField] private AudioClip[] m_mediumClips;
		[SerializeField] private AudioClip[] m_heavyClips;

		[Space]
		[SerializeField] private float m_forceMultiplier = 1;


		private int _previousLightIndex = -1;
		private int _previousMediumIndex = -1;
		private int _previousHeavyIndex = -1;


		public bool PlaySound( Vector3 position, float force, float volumeMultiplier = 1 )
		{
			if( m_lightClips.Length == 0 && m_mediumClips.Length == 0 && m_heavyClips.Length == 0 )
				return false;


			force *= m_forceMultiplier;

			AudioClip clip;
			if( force >= 0.6f && m_heavyClips.Length > 0 )
			{
				_previousLightIndex = -1;
				_previousMediumIndex = -1;
				clip = m_heavyClips.GetRandom( out _previousHeavyIndex, _previousHeavyIndex );
			}
			else
			{
				if( m_lightClips.Length <= 0 )
					throw new ArgumentOutOfRangeException( "Walk Clips count in 0!" );

				_previousMediumIndex = -1;
				_previousHeavyIndex = -1;
				clip = m_lightClips.GetRandom( out _previousLightIndex, _previousLightIndex );
			}

			if( clip != null )
			{
                AudioSourcePoolable audioSource = AudioSourcesPool.GetAudioSource();
				audioSource.clip = clip;
				audioSource.position = position;
				audioSource.volume = force * volumeMultiplier; // * surface.VolumeRandomizeRange.GetRandom();
				audioSource.SetRandomPitch( 0.9f, 1.1f );

				audioSource.PlayOneShot();
			}
			else
				throw new ArgumentOutOfRangeException( "AudioClip is missing!" );

			return clip != null;
		}
	}
}