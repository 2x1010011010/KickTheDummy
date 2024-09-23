using Game.ResourceSystem;
using NodeCanvas.StateMachines;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterEntity", menuName = "StaticData/CharacterEntity", order = 1)]
public class CharacterEntity : PropEntity
{
    [field: SerializeField, BoxGroup("CHARACTER")] public SkinID SkinID { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public AnimatorOverrideController CustomAnimator { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public PropEntity ItemInHand { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public FSM AIBehaviour { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public List<TargetTag> TargetTags { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public List<TargetTag> AllowedDetectTags { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public bool CanInteract { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public bool CanGrab { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public bool CanDrive { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public bool CanKick { get; private set; }
    [field: SerializeField, BoxGroup("CHARACTER")] public bool CanCrouch { get; private set; }
}
