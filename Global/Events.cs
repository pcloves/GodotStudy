using Godot;

namespace GodotStudy.Global;

public partial class Events : Node2D
{
    [Signal]
    public delegate void ChunkChangedEventHandler(Vector2I oldCoord, Vector2I newCoord);

}