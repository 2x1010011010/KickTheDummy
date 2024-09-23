using Sirenix.OdinInspector;
using UnityEngine;

public class Resource : ScriptableObject
{
    [field: SerializeField, ReadOnly] public string ID { get; private set; }

#if UNITY_EDITOR
    [Button("GenerateID", ButtonSizes.Large), BoxGroup("ACTIONS")]
    private void GenerateID()
    {
        if(ID != null)
            if (ID.Length != 0)
                return;

        var giud = UnityEditor.GUID.Generate();
        ID = giud.ToString();
    }
#endif

}
