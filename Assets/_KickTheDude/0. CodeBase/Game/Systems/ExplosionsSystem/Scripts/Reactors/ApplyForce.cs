using System.Linq;
using UnityEngine;

namespace Game.ExplosionSystem
{
    public class ApplyForce : ExplodeReactor
    {
        public override string Name => "APPLY FORCE";

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private ForceParameters _addForceParameters;

        public override void ReactOnExplode(ExplosionData explosionData)
        {
            var rigidbodies = explosionData.ExplosionContacts.Select(x => x.Rigidbody).Distinct();

            foreach (var rigidbody in rigidbodies)
            {
                if (_layerMask == (_layerMask | (1 << rigidbody.gameObject.layer)))
                {
                    rigidbody.AddExplosionForce(_addForceParameters.Force, explosionData.ExplosionPosition, explosionData.ExplosionRadius, 1, _addForceParameters.ForceMode);
                }
            }
        }
    }
}
