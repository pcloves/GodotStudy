shader_type canvas_item;

uniform vec4 mainColor : source_color = vec4(1.0);
uniform sampler2D causticNoiseTexture : hint_default_white, repeat_enable;
uniform sampler2D noiseTexture : hint_default_white, repeat_enable;

void fragment() {
	
	vec2 uv = vec2(UV.x + TIME * 0.1, UV.y + TIME * 0.1);
	vec4 noiseColor = texture(noiseTexture, uv * .05);
	
	vec2 uvOffset = vec2(UV.x + noiseColor.x, UV.y + noiseColor.y);
	vec4 color = texture(causticNoiseTexture, uvOffset);
	
	COLOR = color;
}
