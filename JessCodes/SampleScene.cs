using System;
using System.Globalization;
using System.Linq;
using BetterEnumsGen;
using Godot;
using Godot.Collections;
using GodotStudy.Extensions;

namespace GodotStudy.JessCodes;

public partial class SampleScene : Node2D
{
    public const int TileSize = 16;
    public const int RenderDist = 60;

    [Export] private Vector2I _position = new(0, 0);

    private FastNoiseLite _heightNoise = new();
    private TileMap _tileMap;
    private Sprite2D _heightMap;
    private Node2D _indicator;

    private readonly System.Collections.Generic.Dictionary<Vector2I, TileMeta> _tileCoordMap = new();

    public override void _Ready()
    {
        _tileMap = GetNodeOrNull<TileMap>("%TileMap");
        _indicator = GetNodeOrNull<Node2D>("%Indicator");

        Generate(_position);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton { Pressed: true })
        {
            var localMousePosition = GetLocalMousePosition();
            var tilePosition = _tileMap.LocalToMap(localMousePosition);

            _indicator.Visible = false;
            if (_tileCoordMap.TryGetValue(tilePosition, out var tile))
            {
                _indicator.Visible = true;
                _indicator.Position = tilePosition * TileSize;
                _indicator.GetNodeOrNull<Label>("%Position").Text = $"Position: {tilePosition.ToString()}";
                _indicator.GetNodeOrNull<Label>("%Height").Text = $"Height: {tile!.Height.ToString(CultureInfo.CurrentCulture)}";
                _indicator.GetNodeOrNull<Label>("%ColorLabel").Text = tile!.HeightColor.ToString();
                _indicator.GetNodeOrNull<ColorRect>("%ColorRect").Color = tile!.HeightColor;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("regenerate"))
        {
            GD.Print("regenerate press");
            _position = new Vector2I(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            Generate(_position);
        }

        if (Input.IsActionJustPressed("switch_tilemap"))
        {
            _tileMap.Visible = !_tileMap.Visible;
        }

        if (Input.IsActionJustPressed("switch_heightmap"))
        {
            _heightMap.Visible = !_heightMap.Visible;
        }
    }

    private void Generate(Vector2I tileCoord)
    {
        GD.Print("------------generate------------");

        //先把生成的高度图删除掉
        if (_heightMap != null)
        {
            RemoveChild(_heightMap);
            _heightMap.Free();
            _heightMap = null;
        }

        _indicator.Visible = false;
        _tileCoordMap.Clear();

        //每次重新生成种子
        _heightNoise.Seed = Random.Shared.Next();
        _heightNoise.Frequency = 0.01f;
        _heightNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        _tileMap.Clear();

        //这是生成地图的中心点
        var centerCoordX = tileCoord.X;
        var centerCoordY = tileCoord.Y;

        var waterCells = new Array<Vector2I>();
        //TODO：下面逻辑是生成高度图的逻辑，实际上FastNoiseLite#GetImage可以直接获得噪声图，那这么来说只要把FastNoiseLite#Offset偏移到指定的位置就可以了，所以这里的逻辑可以优化
        var heightImage = Image.Create(RenderDist * 2 + 1, RenderDist * 2 + 1, false, Image.Format.Rgba8);
        for (var i = centerCoordX - RenderDist; i <= centerCoordX + RenderDist; i++)
        {
            for (var j = centerCoordY - RenderDist; j <= centerCoordY + RenderDist; j++)
            {
                var coords = new Vector2I(i, j);
                var height = _heightNoise.GetNoise2D(i, j);

                //小于0，说明是水
                if (height < 0.0f)
                {
                    waterCells.Add(coords);
                }

                //把高度值规范到[0, 1]之间
                var heightColorValue = (float)Mathf.Remap(height, -0.6, 0.6, 0, 1);
                //灰度
                var heightColor = new Color(heightColorValue, heightColorValue, heightColorValue, heightColorValue);
                //生成tile元数据
                _tileCoordMap[coords] = new TileMeta(coords, height, heightColor);

                //设置颜色
                heightImage.SetPixel(i - centerCoordX + RenderDist, j - centerCoordY + RenderDist, heightColor);
            }
        }

        var max = _tileCoordMap.Values.Select(meta => meta.Height).Max();
        var min = _tileCoordMap.Values.Select(meta => meta.Height).Min();

        GD.Print($"max:{max}, min:{min}, seed:{_heightNoise.Seed}");

        //terrain连接！
        var terrainSetWater = Layer.Water.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainWater = Layer.Water.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.Water.ToInt(), waterCells, terrainSetWater, terrainWater);

        var terrainSetWaterBackground = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainWaterBackground = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.WaterBackground.ToInt(), waterCells, terrainSetWaterBackground, terrainWaterBackground);
        
        
        var heightTexture = ImageTexture.CreateFromImage(heightImage);

        _heightMap = new Sprite2D();
        _heightMap.Name = "heightMap";
        _heightMap.Texture = heightTexture;
        _heightMap.Scale = new Vector2(TileSize, TileSize);
        _heightMap.Position = tileCoord * TileSize + new Vector2(TileSize / 2.0f, TileSize / 2.0f);
        _heightMap.Modulate = new Color(Colors.White, 0.3f);

        AddChild(_heightMap);

        var tileSet = _tileMap.TileSet;
        var tileSetSource = (TileSetAtlasSource)tileSet.GetSource(TileSource.Water.ToInt());

        tileSetSource.SetShaderParameter("heightTexture", heightTexture);
        tileSetSource.SetShaderParameter("heightTextureGlobalPosition",
            ToGlobal(new Vector2(-RenderDist * TileSize, -RenderDist * TileSize) + new Vector2(TileSize, TileSize) / 2 + tileCoord * TileSize));
        tileSetSource.SetShaderParameter("heightTextureSize", (2.0f * RenderDist + 1) * TileSize);

        GD.Print("------------generate finish------------");
    }

    private static Vector2I GetChunkCoordinate(Vector2I position)
    {
        var chunkCoordinate = position / TileSize;
        return chunkCoordinate;
    }
}