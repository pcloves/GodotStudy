shader_type canvas_item;

varying vec2 globalPosition;
uniform sampler2D heightTexture : hint_default_white;
uniform vec4 mainColor : source_color  = vec4(1.0);

uniform float heightTextureSize = 176.0f;
uniform vec2 heightTextureGlobalPosition;

void vertex() 
{
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() 
{
	vec4 color = texture(TEXTURE, UV);
	
	COLOR = mainColor;
	COLOR.a *= color.a;
}