#version 330 core

layout (location = 0) in ivec2 aData;

out vec3 vertColor;

//TODO: Add the chunk position uniform. Other option is to have each chunk have their own model matrix
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // Extracting values from the packed integers
    int positionIndex = aData.x & 0xFFFF;        // Extract lower 16 bits
    int lightColor = (aData.x >> 16) & 0x1FF;    // Extract bits 16-24
    int lightLevel = (aData.x >> 25) & 0x1F;     // Extract bits 25-29
    int uvIndex = (aData.x >> 30) & 0x3;         // Extract bits 30-31
    
    int textureIndex = aData.y & 0xFFF;          // Extract lower 12 bits
    int skyLightLevel = (aData.y >> 12) & 0x1F;  // Extract bits 12-16
    int normal = (aData.y >> 17) & 0x7;          // Extract bits 17-19
    
    int x = (positionIndex >> 10) & 0x1F;
    int y = (positionIndex >> 5) & 0x1F;
    int z = positionIndex & 0x1F;
    vec3 position = vec3(x, y, z);
    
    // Define colors for each direction
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
    
    vertColor = normalColor;
    //vertColor = vec3(float(positionIndex) / 35936.0, 0.0, 0.0);
    gl_Position = vec4(position, 1.0) * model * view * projection;
}
