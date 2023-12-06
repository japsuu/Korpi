#version 330 core

uniform sampler2D crosshair;

in vec2 TexCoord;

void main() {
	gl_FragColor = texture(crosshair, TexCoord);
}