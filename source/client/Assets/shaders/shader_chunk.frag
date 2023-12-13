#version 330 core

in vec3 uv;
in vec3 aoColor;
in float faceShading;

out vec4 FragColor;

uniform sampler2DArray texture0;

void main()
{
	FragColor = texture(texture0, uv) * vec4(aoColor * faceShading, 1.0f);
	// FragColor = vec4(outNormalColor, 1.0f);
}
