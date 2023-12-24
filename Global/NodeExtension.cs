using Godot;

namespace GodotStudy.Global;

public static class NodeExtension
{
    public static T Global<T>(this Node node) where T : Node 
    {
        return node.GetNodeOrNull<T>($"/root/{typeof(T).Name}");
    }
}