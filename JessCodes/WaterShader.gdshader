shader_type canvas_item;

varying vec2 globalPosition;
uniform sampler2D gradientMap;
uniform sampler2D heightMap;

void vertex() {
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	vec4 heightValue = texture(heightMap, SCREEN_UV);
	vec4 gradientColor = texture(gradientMap, vec2(heightValue.x, 0.5));
	
	if(COLOR.r == 0.0 && COLOR.a > 0.0)
	{
		COLOR = gradientColor;
	}
}