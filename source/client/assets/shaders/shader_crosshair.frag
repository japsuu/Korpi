#version 420 core

uniform sampler2D crosshair;

in vec2 TexCoord;

out vec4 FragColor;

void main() {
	vec4 color = texture(crosshair, TexCoord);
	FragColor = vec4(color.rgb, color.a * 0.5);
}