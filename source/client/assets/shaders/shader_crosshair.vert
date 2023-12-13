#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform float aspectRatio;

void main() {
	TexCoord = aTexCoord;
	vec2 adjustedPos = aPos;
	adjustedPos.x /= aspectRatio;
	gl_Position = vec4(adjustedPos, 0, 1.0);
}