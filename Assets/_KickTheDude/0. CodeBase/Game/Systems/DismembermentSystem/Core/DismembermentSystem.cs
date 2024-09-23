using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DismembermentSystem : MonoBehaviour
{
    [SerializeField] private AudioSource _mouth;
    [SerializeField] private AudioSource _dismember;
    [SerializeField] private AudioClip[] _moanClips;
    [SerializeField] private AudioClip _dismemberClip;
    [SerializeField] private BehaviourPuppet _puppet;
    [SerializeField] private PuppetMaster _puppetMaster;
    [SerializeField] private FullBodyBipedIK _bipedIK;
    [SerializeField] private List<DismemberableBodyPart> _dismemberableBodyParts;

    private void OnEnable()
    {
        foreach (var dismemberableBodyPart in _dismemberableBodyParts)
            dismemberableBodyPart.Dismembered += DismemberableBodyPartDismembered;
    }

    private void OnDisable()
    {
        foreach (var dismemberableBodyPart in _dismemberableBodyParts)
            dismemberableBodyPart.Dismembered -= DismemberableBodyPartDismembered;
    }

    private void DismemberableBodyPartDismembered(IDismemberable dismemberable, DismemberType dismemberType)
    {
        _dismember.PlayOneShot(_dismemberClip);

        if (dismemberable is Head)
        {
            _puppetMaster.Kill();
            _bipedIK.enabled = false;
        }

        _puppet.Unpin();
        _puppet.UnPin(0, 100);
        _puppet.canGetUp = false;
        _puppetMaster.internalCollisions = true;

        _mouth.PlayOneShot(_moanClips[Random.Range(0, _moanClips.Length)]);
    }
}
