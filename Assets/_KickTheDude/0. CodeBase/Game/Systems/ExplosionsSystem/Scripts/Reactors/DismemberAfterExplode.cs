using System.Linq;
using UnityEngine;

public class DismemberAfterExplode : ExplodeReactor
{
    public override string Name => "DISMEMBER LIMB";

    [SerializeField] private DismemberType _dismemberType;
    [SerializeField] private float _dismemberRadius;

    public override void ReactOnExplode(ExplosionData explosionData)
    {
        var explosionContacts = explosionData.ExplosionContacts.ToArray();

        foreach (var explosionContact in explosionContacts)
        {
            if (explosionContact.DistanceToExplosionCenter > _dismemberRadius) continue;

            var dismemberable = explosionContact.Rigidbody.GetComponent<IDismemberable>();

            if (dismemberable == null) continue;

            dismemberable.Dismember(_dismemberType);
        }
    }
}
