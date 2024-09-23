using UnityEngine;


namespace SurfaceDataSystem
{
	public static class AudioSourcesPool
	{
	    private static readonly ObjectPool<AudioSourcePoolable> _pool = new( InstantiateAudioSource );
		private static int _count;

		private static Transform _audioSourcesParent;
		private static Transform AudioSourcesParent
		{
			get
			{
				if( _audioSourcesParent == null )
				{
					_audioSourcesParent = new GameObject( "audio_sources_pool" ).transform;
					Object.DontDestroyOnLoad( _audioSourcesParent.gameObject );
				}
				return _audioSourcesParent;
			}
		}


		private static AudioSourcePoolable _audioSourcePrefab;
		private static AudioSourcePoolable AudioSourcePrefab
		{ 
			get
			{
				if( _audioSourcePrefab == null )
				{
					GameObject audioSourceGO = new( "audio_source_" + _count );
					_count++;
					audioSourceGO.transform.parent = AudioSourcesParent;
					audioSourceGO.SetActive( false );

					_audioSourcePrefab = audioSourceGO.AddComponent<AudioSourcePoolable>();

					_audioSourcePrefab.spatialBlend = 1;
					_audioSourcePrefab.minDistance = 1.5f;
					_audioSourcePrefab.maxDistance = 30f;

					_audioSourcePrefab.AudioSource.playOnAwake = false;
					_audioSourcePrefab.AudioSource.rolloffMode = AudioRolloffMode.Custom;


					AnimationCurve curve = GenerateCurve( _audioSourcePrefab.minDistance, _audioSourcePrefab.maxDistance );
					_audioSourcePrefab.AudioSource.SetCustomCurve( AudioSourceCurveType.CustomRolloff, curve );

					Pool( _audioSourcePrefab );
				}

				return _audioSourcePrefab;
			}
		}

		public static AudioSourcePoolable GetAudioSource()
		{
			AudioSourcePoolable audioSource = _pool.Get();
			audioSource.SetActive( true );
			return audioSource;
		}


		private static AudioSourcePoolable InstantiateAudioSource()
		{
			AudioSourcePoolable audioSource = Object.Instantiate( AudioSourcePrefab, AudioSourcesParent );
			audioSource.name = "audio_source_" + _count;
			_count++;
			return audioSource;
		}


		private static AnimationCurve GenerateCurve( float minDistance, float maxDistance )
		{
			AnimationCurve curve = new();

			curve.AddKey( minDistance, 1 );
			curve.AddKey( minDistance * 2, 0.5f );
			curve.AddKey( minDistance * 4, 0.25f );
			curve.AddKey( minDistance * 8, 0.125f );
			curve.AddKey( minDistance * 16, 0.025f );
			curve.AddKey( maxDistance, 0f );

			for( int i = 0; i < curve.keys.Length; i++ )
				curve.SmoothTangents( i, 0.5f );

			return curve;
		}


		public static void Pool( AudioSourcePoolable audioSource ) => _pool.Return( audioSource );
	}
}