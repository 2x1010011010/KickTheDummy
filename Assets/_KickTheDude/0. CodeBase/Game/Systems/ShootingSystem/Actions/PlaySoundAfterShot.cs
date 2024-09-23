using UnityEngine;
using Sirenix.OdinInspector;

public class PlaySoundAfterShot : ShotAction
{
    [SerializeField, BoxGroup("SETUP")] private AudioSource _audioSource;
    [SerializeField, BoxGroup("SETUP")] private bool _useRandomSounds;
    [SerializeField, BoxGroup("PARAMETERS"), HideIf("_useRandomSounds")] private AudioClip _shotClip;
    [SerializeField, BoxGroup("PARAMETERS"), ShowIf("_useRandomSounds")] private AudioClip[] _shotClips = new AudioClip[0];

    public override string Name => "PLAY SOUND";

    public override void ReactOnShot(ShotData shotData)
    {
        if (_useRandomSounds)
        {
            _audioSource.PlayOneShot(_shotClips[Random.Range(0, _shotClips.Length)]);
        }
        else
        {
            _audioSource.PlayOneShot(_shotClip);
        }
    }
}
