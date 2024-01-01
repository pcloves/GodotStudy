using System;
using System.Globalization;
using Godot;
using Godot.Collections;
using GodotStudy.Global;

namespace GodotStudy.JessCodes;

public partial class SampleScene : Node2D
{
    public static readonly int TileSize = 16;
    public static readonly int RenderDist = 30;

    [Export] private Vector2I _position = new(0, 0);

    public Vector2I CurrentChunkCoord = GetChunkCoordinate(Vector2I.Zero);
    public Vector2I LastChunkCoord = GetChunkCoordinate(Vector2I.Zero);

    private FastNoiseLite _height = new();
    private TileMap _tileMap;
    private Node2D _indicator;


    private readonly System.Collections.Generic.Dictionary<Vector2I, Tile> _tileCoordMap = new();

    public override void _Ready()
    {
        CurrentChunkCoord = GetChunkCoordinate(_position);
        LastChunkCoord = GetChunkCoordinate(_position);

        _tileMap = GetNodeOrNull<TileMap>("%TileMap");
        _indicator = GetNodeOrNull<Node2D>("%Indicator");

        // this.Global<Events>().EmitSignal(Events.SignalName.ChunkChanged, _lastChunkCoord, _currentChunkCoord);

        Generate(_position);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.Pressed)
            {
                var localMousePosition = GetLocalMousePosition();
                var tilePosition = _tileMap.LocalToMap(localMousePosition);

                if (_tileCoordMap.TryGetValue(tilePosition, out var value))
                {
                    _indicator.Visible = true;
                    _indicator.Position = tilePosition * TileSize;
                    _indicator.GetNodeOrNull<Label>("%Position").Text = tilePosition.ToString();
                    _indicator.GetNodeOrNull<Label>("%Height").Text = value!.Height.ToString(CultureInfo.CurrentCulture);
                    _indicator.GetNodeOrNull<Label>("%ColorLabel").Text = value!.HeightColor.ToString();
                    _indicator.GetNodeOrNull<ColorRect>("%ColorRect").Color = value!.HeightColor;
                }
                else
                {
                    _indicator.Visible = false;
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        CurrentChunkCoord = GetChunkCoordinate(_position);
        if (CurrentChunkCoord != LastChunkCoord)
        {
            this.Global<Events>().EmitSignal(Events.SignalName.ChunkChanged, LastChunkCoord,
                CurrentChunkCoord);
        }

        LastChunkCoord = CurrentChunkCoord;

        if (Input.IsActionPressed("ui_accept"))
        {
            _position = new Vector2I(Random.Shared.Next(-10, 10), Random.Shared.Next(-10, 10));
            Generate(_position);
        }
    }

    private void Generate(Vector2I tileCoord)
    {
        GD.Print("------------generate------------");

        var sprite2DOld = GetNodeOrNull<Sprite2D>("sprite2D");
        if (sprite2DOld != null)
        {
            RemoveChild(sprite2DOld);
        }

        _indicator.Visible = false;
        _tileCoordMap.Clear();
        _height.Seed = Random.Shared.Next();
        _height.Frequency = 0.01f;
        _height.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

        _tileMap.ClearLayer(1);

        var tileCoordX = tileCoord.X;
        var tileCoordY = tileCoord.Y;

        var cells = new Array<Vector2I>();
        var heightImage = Image.Create(RenderDist * 2 + 1, RenderDist * 2 + 1, false, Image.Format.Rgba8);
        for (var i = tileCoordX - RenderDist; i <= tileCoordX + RenderDist; i++)
        {
            for (var j = tileCoordY - RenderDist; j <= tileCoordY + RenderDist; j++)
            {
                var coords = new Vector2I(i, j);
                var height = _height.GetNoise2D(i, j);

                if (height > 0.5f || height < -0.5f)
                {
                    GD.Print($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!height:{height}, coords:{coords}");
                }

                if (height < 0.0f)
                {
                    cells.Add(coords);
                }

                // cells.Add(coords);
                var heightColorValue = 0 + (1 - 0) * ((height - (-0.5f)) / (0.5f - (-0.5f)));
                var heightColor = new Color(heightColorValue, heightColorValue, heightColorValue, heightColorValue);
                _tileCoordMap[coords] = new Tile(coords, height, heightColor);

                heightImage.SetPixel(i - tileCoordX + RenderDist, j - tileCoordY + RenderDist, heightColor);
            }
        }

        GD.Print("------------generate 1------------");
        _tileMap.SetCellsTerrainConnect(1, cells, 0, 0);
        GD.Print("------------generate 2-----------");


        var heightTexture = ImageTexture.CreateFromImage(heightImage);

        var sprite2D = new Sprite2D();

        sprite2D.Name = "sprite2D";
        sprite2D.Texture = heightTexture;
        sprite2D.Scale = new Vector2(TileSize, TileSize);
        sprite2D.Position = tileCoord * TileSize + new Vector2(TileSize / 2.0f, TileSize / 2.0f);
        sprite2D.Modulate = new Color(Colors.White, 0.3f);

        sprite2D.AddToGroup("texture");

        AddChild(sprite2D);

        var material = _tileMap.Material as ShaderMaterial;

        material!.SetShaderParameter("heightTexture", heightTexture);
        material.SetShaderParameter("heightTextureGlobalPosition",
            ToGlobal(new Vector2(-RenderDist * TileSize, -RenderDist * TileSize) + new Vector2(TileSize / 2.0f, TileSize / 2.0f) + tileCoord * TileSize));
        material.SetShaderParameter("heightTextureSize", (2.0f * RenderDist + 1) * TileSize);
        GD.Print("------------generate finish------------");
    }

    private static Vector2I GetChunkCoordinate(Vector2I position)
    {
        var chunkCoordinate = position / TileSize;
        return chunkCoordinate;
    }
}