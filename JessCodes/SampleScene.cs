using System;
using System.Globalization;
using System.Threading.Tasks;
using BetterEnumsGen;
using Godot;
using Godot.Collections;
using GodotStudy.Extensions;

namespace GodotStudy.JessCodes;

public partial class SampleScene : Node2D
{
    public const int TileSize = 16;
    public const int RenderDist = 30;

    [Export] private Vector2I _position = new(0, 0);
    [Export] private FastNoiseLite _heightNoise = new();
    
    private TileMap _tileMap;
    private Sprite2D _heightMap;
    private Node2D _indicator;

    private readonly System.Collections.Generic.Dictionary<Vector2I, TileMeta> _tileCoordMap = new();

    public override async void _Ready()
    {
        _tileMap = GetNodeOrNull<TileMap>("%TileMap");
        _indicator = GetNodeOrNull<Node2D>("%Indicator");

        await Generate(_position);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
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

    public override async void _Process(double delta)
    {
        if (Input.IsActionJustPressed("regenerate"))
        {
            GD.Print("regenerate press");
            _position = new Vector2I(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            await Generate(_position);
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

    private async Task Generate(Vector2I tileCoord)
    {
        GD.Print("------------generate 1------------");

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

        _tileMap.Clear();

        GD.Print("------------generate 2------------");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //这是生成地图的中心点
        var centerCoordX = tileCoord.X;
        var centerCoordY = tileCoord.Y;

        var waterCells = new Array<Vector2I>();
        var sandCells = new Array<Vector2I>();
        var grassCells = new Array<Vector2I>();
        //TODO：下面逻辑是生成高度图的逻辑，实际上FastNoiseLite#GetImage可以直接获得噪声图，那这么来说只要把FastNoiseLite#Offset偏移到指定的位置就可以了，所以这里的逻辑可以优化
        var heightImage = Image.Create(RenderDist * 2 + 1, RenderDist * 2 + 1, false, Image.Format.Rgba8);
        for (var i = centerCoordX - RenderDist; i <= centerCoordX + RenderDist; i++)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            for (var j = centerCoordY - RenderDist; j <= centerCoordY + RenderDist; j++)
            {
                var coords = new Vector2I(i, j);
                var height = _heightNoise.GetNoise2D(i, j);

                sandCells.Add(coords);
                switch (height)
                {
                    //小于0，说明是水
                    case < 0.0f:
                        waterCells.Add(coords);
                        break;
                    case >= 0.05f and <= 0.2f:
                        grassCells.Add(coords);
                        break;
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

        GD.Print("------------generate 3------------");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // var max = _tileCoordMap.Values.Select(meta => meta.Height).Max();
        // var min = _tileCoordMap.Values.Select(meta => meta.Height).Min();
        //
        // GD.Print($"max:{max}, min:{min}, seed:{_heightNoise.Seed}");

        //terrain连接！
        var terrainSetWater = Layer.Water.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainWater = Layer.Water.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.Water.ToInt(), waterCells, terrainSetWater, terrainWater);
        GD.Print("------------generate 4------------");

        var terrainSetWaterBackground = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainWaterBackground = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.WaterBackground.ToInt(), waterCells, terrainSetWaterBackground, terrainWaterBackground);
        GD.Print("------------generate 5------------");

        var terrainSetSand = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainSand = Layer.WaterBackground.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.Sand.ToInt(), sandCells, terrainSetSand, terrainSand);
        GD.Print("------------generate 6------------");

        var terrainSetGrass = Layer.Grass.GetAttributeOfType<TerrainAttribute>()!.TerrainSet;
        var terrainGrass = Layer.Grass.GetAttributeOfType<TerrainAttribute>()!.Terrain;
        _tileMap.SetCellsTerrainConnect(Layer.Grass.ToInt(), grassCells, terrainSetGrass, terrainGrass);
        GD.Print("------------generate 7------------");

        var heightTexture = ImageTexture.CreateFromImage(heightImage);

        _heightMap = new Sprite2D();
        _heightMap.Name = "heightMap";
        _heightMap.Texture = heightTexture;
        _heightMap.Scale = new Vector2(TileSize, TileSize);
        _heightMap.Position = tileCoord * TileSize + new Vector2(TileSize / 2.0f, TileSize / 2.0f);
        _heightMap.Modulate = new Color(Colors.White, 0.3f);

        GD.Print("------------generate 8------------");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        AddChild(_heightMap);

        var tileSet = _tileMap.TileSet;

        var tileSetSourceWater = (TileSetAtlasSource)tileSet.GetSource(TileSource.Water.ToInt());
        tileSetSourceWater.SetShaderParameter("heightTextureSize", (RenderDist * 2 + 1) * TileSize);
        tileSetSourceWater.SetShaderParameter("pixelization", (RenderDist * 2 + 1) * TileSize);
        tileSetSourceWater.SetShaderParameter("heightTexture", heightTexture);
        tileSetSourceWater.SetShaderParameter("heightTextureGlobalPosition",
            ToGlobal(new Vector2(-RenderDist * TileSize, -RenderDist * TileSize) + new Vector2(TileSize, TileSize) / 2 + tileCoord * TileSize));
        tileSetSourceWater.SetShaderParameter("heightTextureSize", (2.0f * RenderDist + 1) * TileSize);

        var tileSetSourceWaterBackground = (TileSetAtlasSource)tileSet.GetSource(TileSource.WaterBackground.ToInt());
        tileSetSourceWaterBackground.SetShaderParameter("heightTextureSize", (RenderDist * 2 + 1) * TileSize);
        tileSetSourceWaterBackground.SetShaderParameter("heightTexture", heightTexture);
        tileSetSourceWaterBackground.SetShaderParameter("heightTextureGlobalPosition",
            ToGlobal(new Vector2(-RenderDist * TileSize, -RenderDist * TileSize) + new Vector2(TileSize, TileSize) / 2 + tileCoord * TileSize));
        tileSetSourceWaterBackground.SetShaderParameter("heightTextureSize", (2.0f * RenderDist + 1) * TileSize);

        var tileSetSourceGrass = (TileSetAtlasSource)tileSet.GetSource(TileSource.Grass.ToInt());
        tileSetSourceGrass.SetShaderParameter("heightTextureSize", (RenderDist * 2 + 1) * TileSize);
        tileSetSourceGrass.SetShaderParameter("heightTexture", heightTexture);
        tileSetSourceGrass.SetShaderParameter("heightTextureGlobalPosition",
            ToGlobal(new Vector2(-RenderDist * TileSize, -RenderDist * TileSize) + new Vector2(TileSize, TileSize) / 2 + tileCoord * TileSize));
        tileSetSourceGrass.SetShaderParameter("heightTextureSize", (2.0f * RenderDist + 1) * TileSize);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GD.Print("------------generate finish------------");
    }

    private static Vector2I GetChunkCoordinate(Vector2I position)
    {
        var chunkCoordinate = position / TileSize;
        return chunkCoordinate;
    }
}