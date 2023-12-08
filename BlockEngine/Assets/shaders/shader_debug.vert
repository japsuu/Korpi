#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out vec3 vertColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 overrideColor;

void main()
{
    /*
    // Define colors for each direction
    vec3 colorXPos = vec3(1.0, 0.0, 0.0);   // Red
    vec3 colorYPos = vec3(0.0, 1.0, 0.0);   // Green
    vec3 colorZPos = vec3(0.0, 0.0, 1.0);   // Blue
    vec3 colorXNeg = vec3(0.0, 1.0, 1.0);   // Light Blue
    vec3 colorYNeg = vec3(1.0, 0.0, 1.0);   // Magenta
    vec3 colorZNeg = vec3(1.0, 1.0, 0.0);   // Yellow
    
    // Normalize the normal vector to ensure it has unit length
    vec3 normalizedNormal = normalize(aNormal);
    
    // Map the normalized normal to the corresponding color
    vec3 finalColor;
    if (normalizedNormal == vec3(1.0, 0.0, 0.0))
        finalColor = colorXPos;
    else if (normalizedNormal == vec3(0.0, 1.0, 0.0))
        finalColor = colorYPos;
    else if (normalizedNormal == vec3(0.0, 0.0, 1.0))
        finalColor = colorZPos;
    else if (normalizedNormal == vec3(-1.0, 0.0, 0.0))
        finalColor = colorXNeg;
    else if (normalizedNormal == vec3(0.0, -1.0, 0.0))
        finalColor = colorYNeg;
    else if (normalizedNormal == vec3(0.0, 0.0, -1.0))
        finalColor = colorZNeg;
    else
        finalColor = vec3(1.0, 1.0, 1.0); // Default to white if the normal doesn't match any expected values
    
    // Assign the color to the output variable
    vertColor = finalColor;
    */
    
    vertColor = aColor * overrideColor;
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}