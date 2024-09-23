using UnityEngine;

public class Head : DismemberableBodyPart
{
    [SerializeField] private AudioSource _mouthSource;

    public override void Dismember(DismemberType dismemberType)
    {
        _mouthSource.gameObject.SetActive(false);

        base.Dismember(dismemberType);
    }
}
