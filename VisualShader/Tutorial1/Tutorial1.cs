using Godot;

namespace Godot4_2Study.VisualShader.Tutorial1;

public partial class Tutorial1 : Control
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
		
		material!.SetShaderParameter("Alpha", value);
	}
}