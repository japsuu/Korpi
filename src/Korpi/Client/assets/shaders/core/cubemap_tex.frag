#version 420 core

in vec3 texCoord0;

layout (binding = 0) uniform samplerCube Sampler0;

out vec4 frag;

void main() {
	vec4 tex = texture(Sampler0, texCoord0);
	frag = tex;
}