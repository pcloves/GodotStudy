shader_type particles;

uniform sampler2D boid_data;
uniform float scale = 0.5;



void process() {
	
	//获取纹理的大小
	ivec2 pos_image_size = textureSize(boid_data, 0);
	
	//获取粒子在纹理上的左边
	ivec2 texel_pos = ivec2(
		int(mod(float(INDEX), float(pos_image_size.x))),  
		int(float(INDEX) / float(pos_image_size.x))
	);
	
	//纹理采样，获得鸟群的位置和旋转，格式为：(x坐标, y坐标, 速度角度（弧度表示）, 0)
	vec4 boid_pos_rot = texelFetch(boid_data, texel_pos, 0);
	
	//生成2D下的旋转矩阵，详情参考：https://docs.godotengine.org/en/stable/tutorials/math/matrices_and_transforms.html#rotating-the-transformation-matrix
	mat2 scale_rot_mat = mat2(
		//第1列，其实就是Transform2D.X
		vec2(cos(boid_pos_rot.z), sin(boid_pos_rot.z)),
		//第2列，其实就是Transform2D.Y
		vec2(-sin(boid_pos_rot.z), cos(boid_pos_rot.z))
	);
	
	//加上缩放
	scale_rot_mat *= scale;
	
	//生成粒子的变换矩阵，详情参考：https://youtu.be/v-xNj4ud0aM?t=211
	/*
	*
	*	| RotZ * ScaleX | RotZ * ScaleY |      0      |  TranslateX  | 
	*	| RotZ * ScaleX | RotZ * ScaleY |      0      |  TranslateY  |
	*	|       0       |       0       |      1      |      0       |
	*	|       0       |       0       |      0      |      1       |
	*	注意：本例中ScaleX和ScaleY相同
	*/
	TRANSFORM[0].xy = scale_rot_mat[0];
	TRANSFORM[1].xy = scale_rot_mat[1];
	TRANSFORM[3].xy = vec2(boid_pos_rot.x, boid_pos_rot.y);
	
}
