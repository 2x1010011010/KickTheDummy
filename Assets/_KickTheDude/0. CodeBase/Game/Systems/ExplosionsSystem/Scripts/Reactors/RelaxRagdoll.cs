using RootMotion.Dynamics;
using System.Linq;

namespace Game.ExplosionSystem
{
    public class RelaxRagdoll : ExplodeReactor
    {
        public override string Name => "RELAX RAGDOLL";

        public override void ReactOnExplode(ExplosionData explosionData)
        {
            ExplosionContact[] explosionContacts = explosionData.ExplosionContacts.ToArray();

            foreach (ExplosionContact explosionContact in explosionContacts)
            {
                if (explosionContact.Rigidbody.transform.lossyScale.x > 2) continue;

                if (explosionContact.Collider.TryGetComponent(out MuscleCollisionBroadcaster relaxable))
                {
                    BehaviourPuppet behaviour = (BehaviourPuppet)relaxable.puppetMaster.behaviours[0];
                    behaviour.Unpin();
                    //relaxable.puppetMaster.muscles[relaxable.muscleIndex].
                    relaxable.Hit(1000000, -explosionContact.ContactNormal * 100, explosionContact.ContactPosition);
                }

                //var relaxable = explosionContact.Rigidbody.GetComponent<IRelaxable>();

                //if (relaxable == null) continue;

                //relaxable.Relax(new RelaxInfo(-1));
            }
        }
    }
}
