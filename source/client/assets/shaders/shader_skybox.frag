#version 330 core

in vec3 TexCoords;
in vec3 SunDirection;
in float SkyboxLerpProgress;

uniform samplerCube skyboxDayTexture;
uniform samplerCube skyboxNightTexture;

// Tint color for sunset/sunrise
const vec3 sunsetTintColor = vec3(1.0, 0.6, 0.0);

out vec4 FragColor;

void main()
{
	vec4 day = texture(skyboxDayTexture, TexCoords);
	vec4 night = texture(skyboxNightTexture, TexCoords);

	// Calculate the angle between the fragment's direction and the sun's direction
	float sunFragmentAngle = dot(normalize(SunDirection), normalize(TexCoords));

	// Tint the sky near the sun during sunset or sunrise
	vec3 tint = mix(vec3(1.0), sunsetTintColor, max(0.0, sunFragmentAngle));

	// Mix the textures based on the time of day
	vec4 final = mix(night, day, SkyboxLerpProgress);

	// Apply the tint
	final.rgb *= tint;

	FragColor = final;
	FragColor = vec4(sunFragmentAngle,0,0,1);
}