using UnityEngine;

public struct ParticlesPaintData
{
    public Vector3 Position;
    public Vector3 Normal;
    public Color Color;
    public float Size;
    public int Amount;

    public ParticlesPaintData(Vector3 position, Vector3 normal, Color color, float size, int amount)
    {
        Position = position;
        Normal = normal;
        Color = color;
        Size = size;
        Amount = amount;
    }
}

public struct TexturePaintData
{
    public Vector3 Position;
    public Vector3 Normal;
    public Texture Texture;
    public float Size;
    public bool ItsNormal;

    public TexturePaintData(Vector3 position, Vector3 normal, Texture texture, float size, bool itsNormal)
    {
        Position = position;
        Normal = normal;
        Texture = texture;
        Size = size;
        ItsNormal = itsNormal;
    }
}

public interface IPaintable
{
    void Paint(ParticlesPaintData paintData);
    void Paint(TexturePaintData paintData);
}
