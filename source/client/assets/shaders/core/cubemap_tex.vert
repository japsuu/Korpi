#version 420 core

layout (location = 0) in vec3 InPosition;

uniform mat4 ModelMat;
uniform mat4 ViewMat;
uniform mat4 ProjMat;

out vec3 texCoord0;

void main() {
    texCoord0 = InPosition;
    // Because OpenTK is in a right-handed coordinate system and OpenGL cubemaps are in a left-handed coordinate system, we need to invert the Y-component of the position vector.
    // See: https://community.khronos.org/t/image-orientation-for-cubemaps-actually-a-very-old-topic/105338/10 , https://opentk.net/learn/chapter1/8-coordinate-systems.html
    vec4 pos = vec4(InPosition.x, -InPosition.y, InPosition.z, 1.0) * ModelMat * ViewMat * ProjMat;
    gl_Position = pos.xyzw;
}
