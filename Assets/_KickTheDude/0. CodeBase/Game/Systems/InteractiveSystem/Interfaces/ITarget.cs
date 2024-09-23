using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetTag
{
    Character,
    Prop
}

public interface ITarget
{
    Transform TargetAimRoot { get; }
    Transform TargetRoot { get; }
    bool Detectable { get; }
    List<TargetTag> TargetTags { get; set; }
}
