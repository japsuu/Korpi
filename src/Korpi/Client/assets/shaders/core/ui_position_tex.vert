#version 420 core

layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec2 UV0;

out vec2 texCoord0;

void main() {
	texCoord0 = UV0;
	gl_Position = vec4(InPosition, 1.0);
}