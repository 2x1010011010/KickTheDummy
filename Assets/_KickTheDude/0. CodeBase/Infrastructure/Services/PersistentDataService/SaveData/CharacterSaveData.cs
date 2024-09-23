using System;

[Serializable]
public class CharacterSaveData : InteractableObjectSaveData
{
    public string GrabbedResourceID;
    public int GrabbedInteractableID;
    public bool isDead;
}
