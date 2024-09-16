shader_type canvas_item;

//内径
uniform float inner_radius = 0.2;
//外径
uniform float outer_radius = 0.4;

//区间映射
float ramp(float input, float in_start, float in_end, float out_start, float out_end) {
	float percent = (input - in_start) / (in_end - in_start);
	return out_start + percent * (out_end - out_start);
}

void fragment() {
	
	//圆心	
	vec2 center = vec2(0.5, 0.5);
	//任意点到圆心的向量
	vec2 vn = UV - center;
	//任意点到圆心的半径
	float radius = length(vn);
	
	//圆环=========================================================
	//内圆
	float inner = smoothstep(inner_radius, inner_radius + 0.005, radius);
	//外圆
	float outer = smoothstep(outer_radius, outer_radius + 0.005, radius);
	//布尔运算，得到圆环：内圆 - 外圆
	//COLOR.a *= abs(inner - outer);
	

	//颜色渐变=========================================================
	//圆环起始向量的角度
	float deg = 0.;
	//转换成弧度
	float theta1 = radians(deg);
	//获取标准园下的坐标，公式为：
	//px = r * cos(theta)
	//py = r * sin(theta)
	vec2 v1 = vec2(cos(theta1), sin(theta1));
	//标准化
	vec2 vn1 = normalize(vn);
	//求得夹角，得到结果为弧度，且范围为： 0 -> PI
	float theta = acos(dot(vn1, v1));
	//映射到：0 -> 1 区间
	//theta2 /= PI;
	//获取起始向量：upvec和当前向量：vn的叉乘
	vec3 n = cross(vec3(v1.x, v1.y, 0.0), vec3(vn.x, vn.y, 0.0));
	//当vn在v1顺时针一侧时，theta2是正值
	//当vn在v1逆时针一侧时，theta2是负值
	theta *= sign(n.z);
	//将弧度从[-PI, PI]映射到[0, 1]
	theta = ramp(theta, -PI, PI, 0., 1.);
	
	//COLOR.rgb *= vec3(theta);
	
	//圆环开口=========================================================
	float inner_start = 0.1;
	float outer_start = 0.9;
	float outer_theta = smoothstep(outer_start, outer_start + 0.001, theta);
	float inner_theta = smoothstep(inner_start, inner_start + 0.001, theta);
	//布尔运算思想!!
	//COLOR.a *= inner_theta - outer_theta;
	
	
	//重复分段=========================================================
	int count = 500;
	float mul = theta * float(count);
	mul = mul - floor(mul);
	
	COLOR.a = mul;
}
