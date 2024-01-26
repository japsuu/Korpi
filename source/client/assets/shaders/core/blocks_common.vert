#version 420 core

layout (location = 0) in uvec2 InData;

uniform mat4 ModelMat;
uniform mat4 ViewMat;
uniform mat4 ProjMat;
uniform vec3 SunDirection;
uniform vec3 ColorModulator;

out vec3 UV;
out vec3 VertexColor;

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
);

vec3 face_normals[6] = vec3[6](
    vec3( 1.0,  0.0,  0.0),  //  X
    vec3( 0.0,  1.0,  0.0),  //  Y
    vec3( 0.0,  0.0,  1.0),  //  Z
    vec3(-1.0,  0.0,  0.0),  // -X
    vec3( 0.0, -1.0,  0.0),  // -Y
    vec3( 0.0,  0.0, -1.0)   // -Z
);

void main()
{
    // Extracting values from the packed integers.
    // First int.
    uint positionIndex = InData.x & 0x3FFFFu;        // Extract lower 18 bits
    uint lightColor = (InData.x >> 18) & 0x1FFu;    // Extract bits 18-26
    uint lightLevel = (InData.x >> 27) & 0x1Fu;     // Extract bits 27-32
    
    // Second int.
    uint textureIndex = InData.y & 0xFFFu;          // Extract lower 12 bits
    uint skyLightLevel = (InData.y >> 12) & 0x1Fu;  // Extract bits 12-16
    uint normal = (InData.y >> 17) & 0x7u;          // Extract bits 17-19
    uint uvIndex = (InData.y >> 20) & 0x3u;         // Extract bits 20-22
    uint aoIndex = (InData.y >> 22) & 0x3u;         // Extract bits 22-24
    
    // Calculate the position of the vertex.
    uint x = (positionIndex >> 12) & 0x3Fu;
    uint y = (positionIndex >> 6) & 0x3Fu;
    uint z = positionIndex & 0x3Fu;
    vec3 position = vec3(x, y, z);

    // Calculate face shading.
    vec3 faceNormal = face_normals[normal];
    float dotProduct = dot(faceNormal, SunDirection);
    float shading;
    // Check if the normal is pointing up or down (Y-axis), to apply constant lighting for top and bottom faces.
    if (faceNormal.y == 1.0 || faceNormal.y == -1.0) {
        const float topShading = 1.0;
        const float bottomShading = 0.5;
        shading = mix(bottomShading, topShading, (faceNormal.y + 1.0) * 0.5);
    } else {
        float dotProduct = dot(faceNormal, SunDirection);
        shading = dotProduct * 0.5 + 0.5;
        shading = shading * 0.3 + 0.7;  // Scale shading to range [0.7, 1]
    }
    
    VertexColor = aoColors[aoIndex] * ColorModulator * shading;
    UV = vec3(textureCoords[uvIndex], textureIndex);
    gl_Position = vec4(position, 1.0) * ModelMat * ViewMat * ProjMat;
}
