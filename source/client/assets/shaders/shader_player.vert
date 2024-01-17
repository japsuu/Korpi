#version 420 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 vColor;

void main() {
	vColor = aColor;
	gl_Position = vec4(aPos, 1.0) * model * view * projection;
}