using Game.InteractiveSystem;
using UnityEngine;

public interface ITool
{
    void StartUse(Vector2 screenPosition, Vector2 direction);
    void StopUse();
}
