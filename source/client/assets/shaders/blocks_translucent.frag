#version 330 core

in vec3 uv;
in vec3 aoColor;
in float faceShading;

out vec4 FragColor;

uniform sampler2DArray texture0;

void main()
{
	vec4 tex = texture(texture0, uv);
	vec3 final = tex.rgb * aoColor * faceShading;
	FragColor = vec4(final, tex.a);
}
