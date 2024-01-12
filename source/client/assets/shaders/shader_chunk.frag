#version 330 core

in vec3 uv;
in vec3 aoColor;
in float faceShading;

out vec4 FragColor;

uniform sampler2DArray texture0;

void main()
{
	vec3 texColor = texture(texture0, uv).rgb;
	vec3 final = texColor * aoColor * faceShading;
	FragColor = vec4(final, 1.0f);
	// FragColor = vec4(outNormalColor, 1.0f);
}
