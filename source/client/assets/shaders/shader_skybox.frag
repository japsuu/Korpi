#version 330 core

in vec3 TexCoords;
in vec3 SunDirection;
in float SkyboxLerpProgress;

uniform samplerCube skyboxDayTexture;
uniform samplerCube skyboxNightTexture;

// Tint color for sunset/sunrise
const vec3 sunsetTintColor = vec3(1.0, 0.6, 0.0);

const vec3 sunBloomColor = vec3(1.0, 0.86, 0.64);

// Range of the bloom in the range 0-1.
const float sunBloomRange = 0.01;
const float sunBloomStrength = 0.7;

out vec4 FragColor;

vec4 sampleSkybox()
{
	vec4 day = texture(skyboxDayTexture, TexCoords);
	vec4 night = texture(skyboxNightTexture, TexCoords);

	// Mix the textures based on the time of day
	return mix(night, day, SkyboxLerpProgress);
}

void main()
{
	float sunriseSunsetProgress = pow(SkyboxLerpProgress, 0.2);
	
	// Calculate the sunDirectionFactor, which is a value between 0 and 1
	float sunDirectionFactor = dot(normalize(SunDirection), normalize(TexCoords));
	sunDirectionFactor = 1 - ((sunDirectionFactor + 1.0) / 2.0);
	
	// Sample the skybox
	vec4 skyboxColor = sampleSkybox();

	// Tint the sky based on the direction to the sun
	vec3 tint = mix(vec3(1.0), sunsetTintColor, sunDirectionFactor);

	// Select the strength based on the sun direction factor
	float sunTintFactor = clamp(sunDirectionFactor * 10.0, 0.0, 1.0);
	
	// Modify the strength to only show the tint during sunset or sunrise (as SkyboxLerpProgress is between 0 and 1)
	sunTintFactor *= sunriseSunsetProgress * (1.0 - sunriseSunsetProgress) * 4;
	
	// Apply the tint factor to the tint
	tint = mix(vec3(1.0), tint, sunTintFactor);
	
	// Apply the skybox color and the tint to the final color
	vec3 final = skyboxColor.rgb * tint;
	
	// Add some bloom around the sun
	float bloomFactor = smoothstep(1.0 - sunBloomRange, 1.0, sunDirectionFactor) * sunBloomStrength;

	// Only show the bloom when sun is visible
	bloomFactor = mix(0.0, bloomFactor, sunriseSunsetProgress);
	
	// Apply the bloom to the final color
	final = mix(final, sunBloomColor, bloomFactor);
	
	// Output the final color
	FragColor = vec4(final, 1.0);
}