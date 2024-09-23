using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IDisposable
{
    event Action<bool> CameraModeActiveChanged;

    Transform Root { get; }
    IReadOnlyDictionary<ToolEntity, ITool> Tools { get; }

    ITool CurentSelectedTool { get; }
    bool CameraModeActive { get; }
    void SetCameraMode(bool active);

    UniTask Initialize();
    void SetActiveTool(ITool tool);
}
