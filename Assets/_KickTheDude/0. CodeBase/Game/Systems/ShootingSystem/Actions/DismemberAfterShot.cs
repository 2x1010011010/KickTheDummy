using UnityEngine;

public class DismemberAfterShot : ShotAction
{
    public override string Name => "DISMEMBER";

    [SerializeField] private DismemberType _dismemberType;

    public override void ReactOnShot(ShotData shotData)
    {
        if (shotData.ShotCollider == null) return;
        if (shotData.ShotCollider.attachedRigidbody == null) return;

        if (shotData.ShotCollider.attachedRigidbody.TryGetComponent(out IDismemberable dismemberable))
            dismemberable.Dismember(_dismemberType);
    }
}
