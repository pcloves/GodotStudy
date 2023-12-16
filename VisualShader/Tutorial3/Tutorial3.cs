using Godot;

namespace GodotStudy.VisualShader.Tutorial3;

[Tool]
public partial class Tutorial3 : Control
{
	private HSlider _hSlider;
	private Sprite2D _sprite2D;
	public override void _Ready()
	{
		_hSlider = GetNodeOrNull<HSlider>("%HSlider");
		_hSlider.ValueChanged += OnHSliderValueChanged;

		_sprite2D = GetNodeOrNull<Sprite2D>("%Sprite2D");

	}

	private void OnHSliderValueChanged(double value)
	{
		var material = _sprite2D.Material as ShaderMaterial;
		
		material!.SetShaderParameter("Speed", value);
	}
}