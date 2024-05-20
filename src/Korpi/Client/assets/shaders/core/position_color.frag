#version 420 core

in vec4 vertexColor;

uniform vec4 ColorModulator;

out vec4 frag;

void main() {
	vec4 color = vertexColor;
	
	if (color.a == 0.0)
		discard;

	frag = color * ColorModulator;
}
