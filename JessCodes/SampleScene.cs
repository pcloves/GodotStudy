using Godot;
using GodotStudy.Global;

namespace GodotStudy.JessCodes;

public partial class SampleScene : Node2D
{
    public static int ChunkSize = 16;
    public static int RenderDist = 5;

    [Export] private Vector2I _position = new(454, 292);

    public Vector2I _currentChunkCoord = GetChunkCoordinate(Vector2I.Zero);
    public Vector2I _lastChunkCoord = GetChunkCoordinate(Vector2I.Zero);

    public override void _Ready()
    {
        _currentChunkCoord = GetChunkCoordinate(_position);
        _lastChunkCoord = GetChunkCoordinate(_position);

        this.Global<Events>().EmitSignal(Events.SignalName.ChunkChanged, _lastChunkCoord, _currentChunkCoord);
    }

    public override void _Process(double delta)
    {
        _currentChunkCoord = GetChunkCoordinate(_position);
        if (_currentChunkCoord != _lastChunkCoord)
        {
            this.Global<Events>().EmitSignal(Events.SignalName.ChunkChanged, _lastChunkCoord,
                _currentChunkCoord);
        }

        _lastChunkCoord = _currentChunkCoord;
    }

    private static Vector2I GetChunkCoordinate(Vector2I position)
    {
        var chunkCoordinate = position / ChunkSize;
        return chunkCoordinate;
    }
}