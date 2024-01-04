shader_type canvas_item;

varying vec2 globalPosition;
uniform sampler2D mainTexture;
uniform sampler2D gradientTexture;
uniform sampler2D heightTexture;
uniform vec2 heightTextureGlobalPosition;
uniform float heightTextureSize = 176.0f;

void vertex() 
{
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() 
{
	vec4 heightColor = texture(heightTexture, (globalPosition - heightTextureGlobalPosition) / heightTextureSize);
	vec4 gradientColor = texture(gradientTexture, vec2(heightColor.a, 0.5));
	vec4 mainColor = texture(mainTexture, UV);
	float alpha = mainColor.r * gradientColor.r;
	
	if(abs(COLOR.r - 0.0) < 0.001 && COLOR.a > 0.0)
	{
		COLOR = gradientColor;
	}
}