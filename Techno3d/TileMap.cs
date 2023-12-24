using Godot;

namespace GodotStudy.Techno3d;

[Tool]
public partial class TileMap : Godot.TileMap
{
    private ShaderMaterial _tilesetMat = ResourceLoader.Load<ShaderMaterial>("res://Techno3d/TileSet.material");

    public override void _Process(double delta)
    {
        _tilesetMat?.SetShaderParameter("globalMousePosition", GetGlobalMousePosition());
        _tilesetMat?.SetShaderParameter("tileSize", TileSet.TileSize);
    }
}