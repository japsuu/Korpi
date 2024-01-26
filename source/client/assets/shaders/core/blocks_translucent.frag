#version 420 core

in vec3 UV;
in vec3 VertexColor;

layout (binding = 15) uniform sampler2DArray Sampler0;

layout (location = 0) out vec4 accum;	// Accumulation buffer
layout (location = 1) out float reveal;	// Revealage buffer

void main()
{
	vec4 tex = texture(Sampler0, UV);

	// weight function
	float weight = clamp(pow(min(1.0, tex.a * 10.0) + 0.01, 3.0) * 1e8 * pow(1.0 - gl_FragCoord.z * 0.9, 3.0), 1e-2, 3e3);

	// store pixel color accumulation
	vec3 color = tex.rgb * VertexColor;
	accum = vec4(color * tex.a, tex.a) * weight;

	// store pixel revealage threshold
	reveal = tex.a;
}
