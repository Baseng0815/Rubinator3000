#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texCoord;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec3 bitangent;

out vec3 pass_fragPos;
out vec2 pass_texCoord;
out mat3 pass_TBNMat;
flat out int instanceID;

uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

// models for plane and cuboid tranformations
uniform mat4 modelMatrix[6];

// models for whole cube transformation
uniform mat4 cubeModelMatrix;

void main() {
	vec4 worldPosition = cubeModelMatrix * modelMatrix[gl_InstanceID] * vec4(position, 1);

	pass_fragPos = worldPosition.xyz;

	vec4 viewPosition = projectionMatrix * viewMatrix * worldPosition;
	
	pass_texCoord = texCoord;

	vec3 T = normalize(vec3(modelMatrix[gl_InstanceID] * vec4(tangent, 0)));
	vec3 B = normalize(vec3(modelMatrix[gl_InstanceID] * vec4(bitangent, 0)));
	vec3 N = normalize(vec3(modelMatrix[gl_InstanceID] * vec4(normal, 0)));
	pass_TBNMat = mat3(T, B, N);
	
	instanceID = gl_InstanceID;

	gl_Position = viewPosition;
}