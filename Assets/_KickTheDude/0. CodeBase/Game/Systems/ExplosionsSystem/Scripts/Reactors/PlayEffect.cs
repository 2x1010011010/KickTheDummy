namespace Game.ExplosionSystem
{
    public class PlayEffect : ExplodeReactor
    {
        public override string Name => "PLAY EFFECT";

        //[SerializeField] private ImpactParticles _effect;

        //ParticleInteractionResult cachedParticleRezult;

        public override void Init(IExplodeable explodeable)
        {
            base.Init(explodeable);

            //cachedParticleRezult = new ParticleInteractionResult() { ParticlesTemplate = _effect };
        }

        public override void ReactOnExplode(ExplosionData reactData)
        {
            //ImpactParticlePool.EmitParticles(cachedParticleRezult, reactData.ExplosionPosition, Vector3.up, 0);
        }
    }
}