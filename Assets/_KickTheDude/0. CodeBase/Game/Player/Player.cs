using Cysharp.Threading.Tasks;
using Game.InteractiveSystem;
using Lean.Touch;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class Player : SerializedMonoBehaviour, IPlayer
{
    public event Action<bool> CameraModeActiveChanged;

    [SerializeField, BoxGroup("SETUP")] public Transform Root { get; private set; }
    [SerializeField, BoxGroup("SETUP")] public Transform ToolsRoot { get; private set; }
    [SerializeField, BoxGroup("SETUP")] public LeanMultiUpdate CameraInputUpdate { get; private set; }

    [SerializeField, BoxGroup("DEBUG"), ReadOnly] public IReadOnlyDictionary<ToolEntity, ITool> Tools { get; private set; }
    [SerializeField, BoxGroup("DEBUG"), ReadOnly] public ITool CurentSelectedTool { get; private set; }

    public bool CameraModeActive { get; private set; }

    private IStaticDataService _staticDataService;
    private IEntitiesFactory<ITool> _toolFactory;

    [Inject]
    private void Construct(IStaticDataService staticDataService, IEntitiesFactory<ITool> toolFactory)
    {
        _staticDataService = staticDataService;
        _toolFactory = toolFactory;
    }

    public async UniTask Initialize()
    {
        var createdToolsDictionary = new Dictionary<ToolEntity, ITool>();

        foreach (var toolResource in _staticDataService.GetAllToolsData())
        {
            var createdTool = await _toolFactory.CreateEntity(toolResource.ToolReference, ToolsRoot);

            createdToolsDictionary.Add(toolResource, createdTool);
        }

        CurentSelectedTool = createdToolsDictionary.First().Value;

        Tools = createdToolsDictionary;
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(LeanFinger leanFinger)
    {
        if (!CameraModeActive)
            CurentSelectedTool?.StartUse(Input.mousePosition, Vector2.zero);
    }

    private void OnFingerUp(LeanFinger leanFinger)
    {
        CurentSelectedTool?.StopUse();
    }

    /*
    private void Update()
    {
        if (CurentSelectedTool == null) return;

        if (Input.GetMouseButtonDown(0)) Debug.Log("CLICK");

        if (Input.GetMouseButtonDown(0) && !CameraModeActive)
            CurentSelectedTool.StartUse(Input.mousePosition, Vector2.zero);

        if (Input.GetMouseButtonUp(0))
            CurentSelectedTool.StopUse();
    }
    */

    public void SetActiveTool(ITool tool)
    {
        Debug.Log("TOOL CHANGED");

        CurentSelectedTool?.StopUse();

        CurentSelectedTool = tool;

        SetCameraMode(false);
    }

    public void Dispose()
    {

    }

    public void SetCameraMode(bool active)
    {
        CameraModeActive = active;
        CameraInputUpdate.enabled = active;

        CameraModeActiveChanged?.Invoke(active);
    }
}
