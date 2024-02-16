shader_type canvas_item;

varying vec2 globalPosition;

uniform sampler2D gradientTexture : hint_default_white;
uniform sampler2D heightTexture : hint_default_white;

uniform vec2 heightTextureGlobalPosition;
uniform float heightTextureSize = 176.0f;

void vertex() 
{
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() 
{
	vec4 mainColor = texture(TEXTURE, UV);
	
	vec4 heightColor = texture(heightTexture, (globalPosition - heightTextureGlobalPosition) / heightTextureSize);
	vec4 gradientColor = texture(gradientTexture, vec2(heightColor.r, 0.5));
	
	gradientColor.a *= mainColor.a;
	
	COLOR = gradientColor;
}