#version 420 core

layout (location = 0) out vec4 frag;

in vec3 uv;
in vec3 aoColor;
in float faceShading;

uniform sampler2DArray texture0;

void main()
{
	vec3 final = texture(texture0, uv).rgb * aoColor * faceShading;
	frag = vec4(final, 1.0f);
	// FragColor = vec4(outNormalColor, 1.0f);
}
