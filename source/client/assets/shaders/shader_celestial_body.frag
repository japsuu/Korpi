#version 420 core

uniform samplerCube cubeTexture;

in vec3 TexCoords;

out vec4 FragColor;

void main()
{
	vec4 tex = texture(cubeTexture, TexCoords);
	FragColor = tex;
}