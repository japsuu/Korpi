#version 420 core

// shader outputs
layout (location = 0) out vec4 accum;
layout (location = 1) out float reveal;

in vec3 uv;
in vec3 aoColor;
in float faceShading;

uniform sampler2DArray texture0;

void main()
{
	vec4 tex = texture(texture0, uv);
	//tex = vec4(tex.rgb * aoColor * faceShading, tex.a);
	
	// weight function
	float weight = clamp(pow(min(1.0, tex.a * 10.0) + 0.01, 3.0) * 1e8 * pow(1.0 - gl_FragCoord.z * 0.9, 3.0), 1e-2, 3e3);

	// store pixel color accumulation
	accum = vec4(tex.rgb * tex.a, tex.a) * weight;

	// store pixel revealage threshold
	reveal = tex.a;
}
