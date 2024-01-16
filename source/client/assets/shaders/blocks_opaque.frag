#version 330 core

in vec3 uv;
in vec3 aoColor;
in float faceShading;

out vec4 FragColor;

uniform sampler2DArray texture0;

void main()
{
	vec3 final = texture(texture0, uv).rgb * aoColor * faceShading;
	FragColor = vec4(final, 1.0f);
	// FragColor = vec4(outNormalColor, 1.0f);
}
