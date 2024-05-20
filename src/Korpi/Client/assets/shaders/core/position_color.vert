#version 420 core

layout (location = 0) in vec3 InPosition;
layout (location = 1) in vec4 InColor;

uniform mat4 ModelMat;
uniform mat4 ViewMat;
uniform mat4 ProjMat;

out vec4 vertexColor;

void main() {
    vertexColor = InColor;
    gl_Position = vec4(InPosition, 1.0) * ModelMat * ViewMat * ProjMat;
}