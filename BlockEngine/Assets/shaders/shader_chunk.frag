#version 330 core

// in vec3 outNormalColor;
in vec2 uv;

out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
	FragColor = texture(texture0, uv);
	// FragColor = vec4(outNormalColor, 1.0f);
}
