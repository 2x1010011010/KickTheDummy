using UnityEngine;
using Sirenix.OdinInspector;

public class PlayEffectAfterShot : ShotAction
{
    [SerializeField, BoxGroup("PARTICLES")] private ParticleSystem[] _particleSystems = new ParticleSystem[0];

    public override string Name => "PLAY PARTICLE";

    public override void ReactOnShot(ShotData shotData)
    {
        foreach (var particleSystem in _particleSystems)
            particleSystem.Play();
    }
}