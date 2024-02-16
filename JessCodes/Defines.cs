using System;

namespace GodotStudy.JessCodes;

[AttributeUsage(AttributeTargets.Field)]
public class TerrainAttribute : Attribute
{
    public int TerrainSet { get; }
    public int Terrain { get; }

    public TerrainAttribute(int terrainSet, int terrain)
    {
        TerrainSet = terrainSet;
        Terrain = terrain;
    }
}

public enum Layer
{
    [Terrain(0, 0)] Sand = 0,
    [Terrain(0, 1)] WaterBackground = 1,
    [Terrain(0, 2)] Water = 2,
    [Terrain(0, 3)] Grass = 3,
}

public enum TileSource
{
    Sand = 0,
    WaterBackground = 1,
    Water = 2,
    Grass = 3,
}