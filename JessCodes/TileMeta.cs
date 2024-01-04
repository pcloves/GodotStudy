using System.Diagnostics.CodeAnalysis;
using Godot;

namespace GodotStudy.JessCodes;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class TileMeta
{
    public TileMeta(Vector2I coords, float height, Color heightColor)
    {
        Coords = coords;
        Height = height;
        HeightColor = heightColor;
    }

    public Vector2I Coords { get; private set; }
    public float Height { get; private set; }

    public Color HeightColor { get; private set; }
}