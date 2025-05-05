shader_type canvas_item;

//内径
uniform float r1 = 0.2;
//外径
uniform float r2 = 0.4;

void fragment() {

	//圆心
	vec2 center = vec2(0.5, 0.5);
	//圆心到任意点的向量
	vec2 vn = UV - center;
	//任意点到圆心的半径
	float radius = length(vn);

	//内圆
	float radius1 = smoothstep(r1, r1 + 0.01, radius);
	//外圆
	float radius2 = smoothstep(r2, r2 + 0.01, radius);

	//内圆 - 外圆 得到圆环
	COLOR.a = abs(radius1 - radius2);
}
