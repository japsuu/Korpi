#version 330 core

in vec3 vertColor;

out vec4 FragColor;

void main()
{
	FragColor = vec4(vertColor, 1.0f);
}
