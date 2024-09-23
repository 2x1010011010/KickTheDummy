using FluidFlow;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class SimplePaintable : MonoBehaviour
{
    [SerializeField, BoxGroup("SETUP")] private FFSimulatorParticle _simulatorParticle;
    [SerializeField, BoxGroup("SETUP")] private FFCanvas _canvas;
    [SerializeField, BoxGroup("SETUP")] private List<PaintableSender> _paintableSenders;

    private FFDecal decal = new FFDecal(new FFDecal.Channel[1]);

    private void OnEnable()
    {
        foreach (var paintableSender in _paintableSenders)
        {
            paintableSender.PaintParticlesEvent += PaintableSenderPaintParticlesEvent;
            paintableSender.PaintTexturesEvent += PaintableSenderPaintTexturesEvent;
        }
    }

    private void OnDisable()
    {
        foreach (var paintableSender in _paintableSenders)
        {
            paintableSender.PaintParticlesEvent -= PaintableSenderPaintParticlesEvent;
            paintableSender.PaintTexturesEvent -= PaintableSenderPaintTexturesEvent;
        }
    }

    private void PaintableSenderPaintParticlesEvent(ParticlesPaintData paintData)
    {
        Paint(paintData);
    }

    private void PaintableSenderPaintTexturesEvent(TexturePaintData texturePaintData)
    {
        PaintDecal(texturePaintData);
    }

    public void Paint(ParticlesPaintData paintData)
    {
        _simulatorParticle?.ProjectParticles(GetProjection(paintData.Position, paintData.Normal, paintData.Size), new Vector2(.4f, 1f), new Vector2(.5f, 3f), paintData.Color, .6f, paintData.Amount);
    }

    private void PaintDecal(TexturePaintData texturePaintData)
    {
        if (texturePaintData.ItsNormal)
        {
            var decal = new FFDecal(FFDecal.Mask.TextureMask(texturePaintData.Texture, ComponentMask.All), FFDecal.Channel.Normal("Normal", texturePaintData.Texture));
            var projection = GetProjection(texturePaintData.Position, texturePaintData.Normal, texturePaintData.Size);

            _canvas.ProjectDecal(decal, projection);
        }
        else
        {
            //Debug.Log("Try paint " + texturePaintData.Texture.name);
            var decal = new FFDecal(FFDecal.Mask.AlphaMask(texturePaintData.Texture), FFDecal.Channel.Color("Color", texturePaintData.Texture));
            //var decal = new FFDecal(FFDecal.Channel.Color("Color", texturePaintData.Texture));
            var projection = GetProjection(texturePaintData.Position, texturePaintData.Normal, texturePaintData.Size);

            _canvas.ProjectDecal(decal, projection);
        }
    }

    private FFProjector GetProjection(Vector3 position, Vector3 normal, float size)
    {
        var ray = new Ray(position, -normal);
        return FFProjector.Orthogonal(ray, Vector3.up, size, size, -0.1f, 0.1f);
    }
}
