#version 420 core

in vec3 uv;
in vec3 aoColor;
in float faceShading;

out vec4 FragColor;

uniform sampler2DArray texture0;

void main()
{
	vec4 tex = texture(texture0, uv);
	if (tex.a < 0.1f)
		discard;
	vec3 final = tex.rgb * aoColor * faceShading;
	FragColor = vec4(final, 1.0f);
	// FragColor = vec4(outNormalColor, 1.0f);
}
