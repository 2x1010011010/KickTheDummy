using UnityEngine;
using System.Collections;
using System;

[ExecuteAlways]
public class BFX_ShaderProperies : MonoBehaviour {

    public BFX_BloodSettings BloodSettings;

    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public float TimeDelay = 0;

    private bool canUpdate;
    bool isFrized;
    private float startTime;

    private int cutoutPropertyID;
    int forwardDirPropertyID;
    float _animationTimeLapsed;
    private float _timeLapsedBeforeFadeout;

    private MaterialPropertyBlock props;
    private Renderer rend;

    public event Action OnAnimationFinished;

    private void OnEnable()
    {
        if (props == null)
        {
            props = new MaterialPropertyBlock();
            rend = GetComponent<Renderer>();

            cutoutPropertyID = Shader.PropertyToID("_Cutout");
            forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");
        }

        startTime = Time.time + TimeDelay;
        canUpdate = true;

        GetComponent<Renderer>().enabled = true;

        rend.GetPropertyBlock(props);

        var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        props.SetFloat(cutoutPropertyID, eval);
        props.SetVector(forwardDirPropertyID, transform.up);
        rend.SetPropertyBlock(props);
    }

    private void OnDisable()
    {
        rend.GetPropertyBlock(props);

        var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        props.SetFloat(cutoutPropertyID, eval);

        rend.SetPropertyBlock(props);
        _animationTimeLapsed = 0;
        _timeLapsedBeforeFadeout = 0;
    }



    private void Update()
    {
        if (!canUpdate && Application.isPlaying) return;

        rend.GetPropertyBlock(props);
        
        var deltaTime = BloodSettings == null ? Time.deltaTime : Time.deltaTime * BloodSettings.AnimationSpeed;
        _timeLapsedBeforeFadeout += deltaTime;

        if (BloodSettings == null || _timeLapsedBeforeFadeout > BloodSettings.DecalLifeTimeSeconds - 15f || (_animationTimeLapsed / GraphTimeMultiplier) < 0.3f)
            _animationTimeLapsed += deltaTime;
       

        if (!Application.isPlaying && BloodSettings != null)
        {
            if (BloodSettings.DebugAnimationTime < TimeDelay) _animationTimeLapsed = 0;
            else _animationTimeLapsed = BloodSettings.DebugAnimationTime;
        }

        var eval = FloatCurve.Evaluate(_animationTimeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;

        props.SetFloat(cutoutPropertyID, eval);

        if (BloodSettings != null) props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));

        if (_animationTimeLapsed >= GraphTimeMultiplier)
        {
            canUpdate = false;
            OnAnimationFinished?.Invoke();

        }
        props.SetVector(forwardDirPropertyID, transform.up);
        rend.SetPropertyBlock(props);
    }

}
