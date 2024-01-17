#version 420 core

layout (location = 0) in vec3 aPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 sunDirection;
uniform float skyboxLerpProgress;

out vec3 TexCoords;
out vec3 SunDirection;
out float SkyboxLerpProgress;

void main()
{
    TexCoords = aPos;
    SunDirection = sunDirection;
    SkyboxLerpProgress = skyboxLerpProgress;
    // Because OpenTK is in a right-handed coordinate system and OpenGL cubemaps are in a left-handed coordinate system, we need to invert the Y-component of the position vector.
    // Additionally since we are viewing the cubemap "inside-out", we flip the x-component also. This flips the textures.
    // See: https://community.khronos.org/t/image-orientation-for-cubemaps-actually-a-very-old-topic/105338/10 , https://opentk.net/learn/chapter1/8-coordinate-systems.html
    vec4 pos = vec4(-aPos.x, -aPos.y, aPos.z, 1.0) * model * view * projection;
    gl_Position = pos.xyww;
}
