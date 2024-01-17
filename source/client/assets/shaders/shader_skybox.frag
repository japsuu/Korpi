﻿#version 420 core

in vec3 TexCoords;
in vec3 SunDirection;
in float SkyboxLerpProgress;

uniform samplerCube skyboxDayTexture;
uniform samplerCube skyboxNightTexture;

// Tint color for sunset/sunrise
const vec3 sunsetTintColor = vec3(1.0, 0.6, 0.0);

const vec3 sunBloomColor = vec3(1.0, 0.96, 0.84);
const vec3 moonBloomColor = vec3(0.9, 0.9, 1.0);

// Range of the bloom in the range 0-1.
const float sunBloomRange = 0.03;
const float sunBloomStrength = 0.4;

// Range of the bloom in the range 0-1.
const float moonBloomRange = 0.01;
const float moonBloomStrength = 0.7;

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
	float sunriseSunsetProgress = pow(SkyboxLerpProgress, 0.5);
	
	// Calculate the sunDirectionFactor, which is a value between 0 and 1
	float sunDirectionFactor = dot(normalize(SunDirection), normalize(TexCoords));
	float moonDirectionFactor = ((sunDirectionFactor + 1.0) / 2.0);
	sunDirectionFactor = 1 - moonDirectionFactor;
	
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
	float sunBloomFactor = smoothstep(1.0 - sunBloomRange, 1.0, sunDirectionFactor) * sunBloomStrength;
	
	// Add some bloom around the moon
	float moonBloomFactor = smoothstep(1.0 - moonBloomRange, 1.0, moonDirectionFactor) * moonBloomStrength;

	// Only show the sun bloom when sun is visible
	sunBloomFactor = sunBloomFactor * sunriseSunsetProgress;
	
	// Apply the bloom to the final color
	final = mix(final, sunBloomColor, sunBloomFactor);
	final = mix(final, moonBloomColor, moonBloomFactor);
	
	// Output the final color
	FragColor = vec4(final, 1.0);
}