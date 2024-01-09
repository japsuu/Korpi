#version 330 core

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

void main()
{
    TexCoords = aPos;
    // Because OpenTK is in a right-handed coordinate system and OpenGL cubemaps are in a left-handed coordinate system, we need to invert the Y-component of the position vector.
    // See: https://community.khronos.org/t/image-orientation-for-cubemaps-actually-a-very-old-topic/105338/10 , https://opentk.net/learn/chapter1/8-coordinate-systems.html
    vec4 pos = vec4(aPos.x, -aPos.y, aPos.z, 1.0) * model * view * projection;
    gl_Position = pos.xyzw;
}
