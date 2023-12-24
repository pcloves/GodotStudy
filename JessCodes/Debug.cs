using Godot;
using GodotStudy.Global;

namespace GodotStudy.JessCodes;

public partial class Debug : Node2D
{
    private Vector2I _currentChunkCoord;

    public override void _Ready()
    {
        base._Ready();

        this.Global<Events>().ChunkChanged += OnChunkChanged;
    }

    private void OnChunkChanged(Vector2I oldCoord, Vector2I newCoord)
    {
        _currentChunkCoord = newCoord;

        QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();

        var rectPosition = _currentChunkCoord * SampleScene.ChunkSize - new Vector2I(SampleScene.ChunkSize, SampleScene.ChunkSize) * SampleScene.RenderDist;
        var rectSize = new Vector2I((SampleScene.RenderDist * 2 + 1) * SampleScene.ChunkSize, (SampleScene.RenderDist * 2 + 1) * SampleScene.ChunkSize);
        var rect = new Rect2(rectPosition, rectSize);

        GD.Print($"rect:{rect}");

        DrawRect(rect, Colors.White, false, 2);
    }
}