shader_type spatial;

uniform sampler2D normal_map;

uniform int number_of_colors;
uniform vec3[6] colors;
uniform float blend_area;

uniform float max_height;
uniform uint width;
uniform uint depth;

varying vec2 tex_position;
varying float height;

float inverselerp(float minV, float maxV, float v)
{
	return (v - minV)/(maxV - minV);
}

void vertex() {
	height = inverselerp(0.f, max_height, VERTEX.y);
	
	tex_position = vec2(VERTEX.x/float(width), VERTEX.z/float(depth)) * 0.5 + 0.5;
	
	VERTEX = vec3(VERTEX.x, clamp(VERTEX.y, 0, max_height), VERTEX.z);
}

void fragment() {
	vec3 color;
	
	if(height > 0.f)
	{
		float color_step = 1.0 / float(number_of_colors);
        
        uint current_color = uint(floor(height / color_step));
        uint next_color = clamp(current_color + 1u, 0u, uint(number_of_colors - 1));
        uint prev_color = clamp(current_color - 1u, 0u, uint(number_of_colors - 1));
		
        float current_color_height = float(current_color)*color_step;
        float next_color_height = float(next_color)*color_step;
		
		float area = blend_area * color_step * .5f;
		float lower_blend_area = current_color_height + area;
		float upper_blend_area = next_color_height - area;
		
		if(height > current_color_height && height <= lower_blend_area)
		{
			float t = inverselerp(current_color_height, lower_blend_area, height);
			color = mix(colors[prev_color], colors[current_color], mix(.5, 1, t));
			//color = vec3(1, 0 ,0);
		}
		else if(height <= next_color_height && height > upper_blend_area)
		{
			float t = inverselerp(upper_blend_area, next_color_height, height);
			color = mix(colors[current_color], colors[next_color], mix(0, .5, t));
			//color = vec3(1, 1 ,0);
		}
		else
			color = colors[current_color];
		
	}
	else
		color = vec3(0.f, 0.f, 1.f + clamp(height, -.8f, 0));
	
	ALBEDO = color;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
