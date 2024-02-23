#version 420 core

in vec3 UV;
in vec3 VertexColor;

layout (binding = 15) uniform sampler2DArray Sampler0;

layout (location = 0) out vec4 frag;

void main() {
	vec4 tex = texture(Sampler0, UV);
	if (tex.a < 0.5f)
		discard;
	frag = vec4(tex.rgb * VertexColor, tex.a);
}
