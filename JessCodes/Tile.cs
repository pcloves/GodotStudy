using Godot;

namespace GodotStudy.JessCodes;

public class Tile
{
    public Tile(Vector2I coords, float height, Color heightColor)
    {
        Coords = coords;
        Height = height;
        HeightColor = heightColor;
    }

    public Vector2I Coords { get; private set; }
    public float Height { get; private set; }
    
    public Color HeightColor { get; private set; }
}