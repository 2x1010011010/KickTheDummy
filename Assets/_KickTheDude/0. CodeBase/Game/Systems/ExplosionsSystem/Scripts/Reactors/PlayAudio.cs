using UnityEngine;

namespace Game.ExplosionSystem
{
    public class PlayAudio : ExplodeReactor
    {
        public override string Name => "PLAY AUDIO";

        //[SerializeField] private ImpactAudioSource _audioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private float _pitch = 1;
        [SerializeField] private float _volume = 1;

        //AudioInteractionResult cachedAudioRezult;

        public override void Init(IExplodeable explodeable)
        {
            base.Init(explodeable);

            //cachedAudioRezult = new AudioInteractionResult() { AudioSourceTemplate = _audioSource, AudioClip = _clip, Pitch = _pitch, Volume = _volume };
        }

        public override void ReactOnExplode(ExplosionData explosionData)
        {
            //ImpactAudioPool.PlayAudio(cachedAudioRezult, explosionData.ExplosionPosition, 0);
        }
    }
}