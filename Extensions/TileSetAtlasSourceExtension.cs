using Godot;

namespace GodotStudy.Extensions;

public static class TileSetAtlasSourceExtension
{
    public static void SetShaderParameter(this TileSetAtlasSource tileSetAtlasSource, StringName param, Variant value)
    {
        var tilesCount = tileSetAtlasSource.GetTilesCount();
        for (var i = 0; i < tilesCount; i++)
        {
            var tileAtCoords = tileSetAtlasSource.GetTileId(i);
            var tileData = tileSetAtlasSource.GetTileData(tileAtCoords, 0);

            if (tileData.Material is ShaderMaterial tileDataMaterial) tileDataMaterial.SetShaderParameter(param, value);
        }
    }
}