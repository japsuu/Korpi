#version 420 core

in vec2 texCoord0;

layout (binding = 0) uniform sampler2D Sampler0;
//uniform vec4 ColorModulator;

out vec4 frag;

void main() {
	vec4 tex = texture(Sampler0, texCoord0);
	frag = tex /** ColorModulator*/;
}