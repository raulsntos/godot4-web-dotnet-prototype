shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx, unshaded, shadows_disabled;

uniform sampler2D albedo_texture  : source_color, filter_linear_mipmap, repeat_enable;

uniform vec3 uv_scale;
uniform vec3 uv_offset;

void vertex() {
	UV = UV * uv_scale.xy + uv_offset.xy;
}

void fragment() {
	ALBEDO = texture(albedo_texture, UV).rgb;
}
