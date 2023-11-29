#version 330 core

layout (location = 0) in ivec2 aData;

out vec3 uv;
out vec3 aoColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

// Create an array of texture coordinates, that can be indexed with the uvIndex:
vec2 textureCoords[4] = vec2[](
    vec2(0.0, 0.0),
    vec2(1.0, 0.0),
    vec2(1.0, 1.0),
    vec2(0.0, 1.0)
);

vec3 aoColors[4] = vec3[](
    vec3(0.5, 0.5, 0.5),
    vec3(0.7, 0.7, 0.7),
    vec3(0.9, 0.9, 0.9),
    vec3(1.0, 1.0, 1.0)
    // vec3(1.0, 0.0, 0.0),
    // vec3(0.0, 1.0, 0.0),
    // vec3(0.0, 0.0, 1.0),
    // vec3(1.0, 0.0, 1.0)
);

void main()
{
    // Extracting values from the packed integers.
    // First int.
    int positionIndex = aData.x & 0x3FFFF;        // Extract lower 18 bits
    int lightColor = (aData.x >> 18) & 0x1FF;    // Extract bits 18-26
    int lightLevel = (aData.x >> 27) & 0x1F;     // Extract bits 27-32
    
    // Second int.
    int textureIndex = aData.y & 0xFFF;          // Extract lower 12 bits
    int skyLightLevel = (aData.y >> 12) & 0x1F;  // Extract bits 12-16
    int normal = (aData.y >> 17) & 0x7;          // Extract bits 17-19
    int uvIndex = (aData.y >> 20) & 0x3;         // Extract bits 20-22
    int aoIndex = (aData.y >> 22) & 0x3;         // Extract bits 22-24
    
    // Calculate the position of the vertex.
    int x = (positionIndex >> 12) & 0x3F;
    int y = (positionIndex >> 6) & 0x3F;
    int z = positionIndex & 0x3F;
    vec3 position = vec3(x, y, z);
    
    /*// Define colors for each direction
    vec3 colorXPos = vec3(1.0, 0.0, 0.0);   // Red
    vec3 colorYPos = vec3(0.0, 1.0, 0.0);   // Green
    vec3 colorZPos = vec3(0.0, 0.0, 1.0);   // Blue
    vec3 colorXNeg = vec3(0.0, 1.0, 1.0);   // Light Blue
    vec3 colorYNeg = vec3(1.0, 0.0, 1.0);   // Magenta
    vec3 colorZNeg = vec3(1.0, 1.0, 0.0);   // Yellow
    
    // Map the normalized normal to the corresponding color
    vec3 normalColor;
    if (normal == 0)
        normalColor = colorXPos;
    else if (normal == 1)
        normalColor = colorYPos;
    else if (normal == 2)
        normalColor = colorZPos;
    else if (normal == 3)
        normalColor = colorXNeg;
    else if (normal == 4)
        normalColor = colorYNeg;
    else if (normal == 5)
        normalColor = colorZNeg;
    else
        normalColor = vec3(1.0, 1.0, 1.0);
    
    outNormalColor = normalColor;*/
    
    aoColor = aoColors[aoIndex];
    uv = vec3(textureCoords[uvIndex], textureIndex);
    gl_Position = vec4(position, 1.0) * model * view * projection;
}
