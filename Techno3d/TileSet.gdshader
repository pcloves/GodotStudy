shader_type canvas_item;

varying flat vec2 globalPosition;
uniform vec2 globalMousePosition;
uniform vec2 tileSize = vec2(16, 16);

void vertex() {
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	float isWithinX = step(globalPosition.x, globalMousePosition.x) * step(globalMousePosition.x, globalPosition.x + tileSize.x);
	float isWithinY = step(globalPosition.y, globalMousePosition.y) * step(globalMousePosition.y, globalPosition.y + tileSize.y);
	
	vec4 textureColor = texture(TEXTURE, UV);
	
	COLOR = mix(textureColor, vec4(1, 1, 1, 1), 0.5 * isWithinX * isWithinY);
}

