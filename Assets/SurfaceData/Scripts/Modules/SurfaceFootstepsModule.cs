using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
	public class SurfaceFootstepsModule : SurfaceModule
	{
		[SerializeField] private AudioClip[] m_walkClips;
		[SerializeField] private AudioClip[] m_runClips;

		[Space, SerializeField, Range( 0f, 1f ) ] private float m_runThreshold = 0.6f;


		private int _previousRunIndex = -1;
		private int _previousWalkIndex = -1;
			

		public void PlaySound( Vector3 position, float strenght )
		{
			AudioClip clip;
			if( strenght >= m_runThreshold && m_runClips.Length > 0 )
			{
				_previousWalkIndex = -1;
				clip = m_runClips.GetRandom( out _previousRunIndex, _previousRunIndex );
			}
			else
			{
				if( m_walkClips.Length <= 0 )
					throw new ArgumentOutOfRangeException( "Walk Clips count in 0!" );

				_previousRunIndex = -1;
				clip = m_walkClips.GetRandom( out _previousWalkIndex, _previousWalkIndex );
			}

			if( clip != null )
			{
				AudioSourcePoolable audioSource = AudioSourcesPool.GetAudioSource();
				audioSource.clip = clip;
				audioSource.position = position;
				audioSource.volume = strenght;
				audioSource.SetRandomPitch( 0.9f, 1.1f );

				audioSource.PlayOneShot();
			}
			else
				throw new ArgumentOutOfRangeException( "AudioClip is missing!" );
		}
	}
}