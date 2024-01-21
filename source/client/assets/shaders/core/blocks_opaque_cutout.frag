#version 420 core

in vec3 UV;
in vec3 AOColor;
in float FaceShading;

layout (binding = 15) uniform sampler2DArray Sampler0;
//uniform vec4 ColorModulator;

layout (location = 0) out vec4 frag;

void main() {
	vec4 tex = texture(Sampler0, UV) /** ColorModulator*/;
	if (tex.a < 0.5f)
		discard;
	frag = vec4(tex.rgb * AOColor * FaceShading, tex.a);
}
