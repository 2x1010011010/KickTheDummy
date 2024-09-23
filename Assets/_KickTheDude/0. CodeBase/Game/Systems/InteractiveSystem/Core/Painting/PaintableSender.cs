using System;
using UnityEngine;

public class PaintableSender : MonoBehaviour, IPaintable
{
    public event Action<ParticlesPaintData> PaintParticlesEvent;
    public event Action<TexturePaintData> PaintTexturesEvent;

    public void Paint(ParticlesPaintData paintData)
    {
        PaintParticlesEvent?.Invoke(paintData);
    }

    public void Paint(TexturePaintData paintData)
    {
        PaintTexturesEvent?.Invoke(paintData);
    }
}
