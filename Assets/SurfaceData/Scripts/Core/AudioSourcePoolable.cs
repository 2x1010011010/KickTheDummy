using SurfaceDataSystemOld;
using System.Collections;
using UnityEngine;


namespace SurfaceDataSystem
{
    [RequireComponent( typeof( AudioSource ) )]
    public class AudioSourcePoolable : Poolable
    {
        private AudioSource _audioSource;
        public AudioSource AudioSource
        {
            get
            {
                if( !_audioSource )
                    TryGetComponent( out _audioSource );
                return _audioSource;
            }
        }
    
        private Transform _transform;
        public Transform Transform
        {
            get
            {
                if( !_transform )
                    TryGetComponent( out _transform );
                return _transform;
            }
        }
    
    
        public Vector3 position
        {
            get => Transform.position;
            set => Transform.position = value;
        }
    
        public AudioClip clip
        {
            get => AudioSource.clip;
            set => AudioSource.clip = value;
        }

        public bool fading;
        

        private float _targetVolume;
        public float volume
        {
            get => AudioSource.volume;
            set
            {
                if( smoothVolume )
                    _targetVolume = value;
                else
                    AudioSource.volume = value;
            }
        }
    
        public float spatialBlend
        {
            get => AudioSource.spatialBlend;
            set => AudioSource.spatialBlend = value;
        }
    
        public float minDistance
        {
            get => AudioSource.minDistance;
            set => AudioSource.minDistance = Mathf.Max( value, 0.1f );
        }
        
        public float maxDistance
        {
            get => AudioSource.maxDistance;
            set => AudioSource.maxDistance = Mathf.Max( minDistance, value );
        }

        public bool loop
        {
            get => AudioSource.loop;
            set => AudioSource.loop = value;
        }

        public float playtime
        { 
            get => AudioSource.time / clip.length;
            set
            {
                float v = ( value * clip.length ) / pitch;
                if( float.IsNaN( v ) )
                    v = 0;

				AudioSource.time = v;
			}
        }


		public float pitch
		{
			get => AudioSource.pitch;
			set => AudioSource.pitch = value;
		}


		public bool smoothVolume;
    
        public bool RandomizePitch { get; set; }
    
        public void SetActive( bool value ) => gameObject.SetActive( value);
        public void SetRandomPitch( float min, float max ) => pitch = Random.Range( min, max );
        public void SetRandomPitch( Range range ) => pitch = range.GetRandom();
    
    
        public void PlayOneShot( AudioClip clip )
        {
            this.clip = clip;
            StartCoroutine( PlayOneShotCoroutine() );
        }
    
        public void PlayOneShot() => StartCoroutine( PlayOneShotCoroutine() );
    
        private IEnumerator PlayOneShotCoroutine()
        {
            AudioSource.Stop();
            if( RandomizePitch )
                SetRandomPitch( 0.9f, 1.1f );

			AudioSource.Play();

            yield return new WaitForSeconds( clip.length );
            Pool();
        }


        public void Play() => AudioSource.Play();

        public void FadeAway( float duration )
        {
            smoothVolume = false;
            if( duration <= 0 )
                Pool();
            else
                StartCoroutine( FadeAwayCoroutine( duration ) );
        }

        private IEnumerator FadeAwayCoroutine( float duration )
        {
            float t = 0;
            float startVolume = volume;
            while( t < duration )
            {
                t += Time.deltaTime;
                volume = Mathf.Lerp( startVolume, 0, t / duration );
                yield return null;
            }

            Pool();
        }


        private void Pool()
        {
            SetActive( false );
            AudioSource.Stop();
            loop = false;
            volume = 0;
            pitch = 1;
            smoothVolume = false;
            playtime = 0;
            AudioSourcesPool.Pool( this );
        }


		protected override void Update()
		{
			if( smoothVolume )
                UpdateSmoothVolume();
		}


        private void UpdateSmoothVolume()
        {
            AudioSource.volume = Mathf.Lerp( AudioSource.volume, _targetVolume, 10 * Time.deltaTime );
        }

		public static implicit operator AudioSource( AudioSourcePoolable poolable ) => poolable.AudioSource;
    }
}