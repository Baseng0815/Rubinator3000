﻿#version 300 es

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texCoord;

out vec2 pass_texCoord;
flat out int instanceID;

uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

// models for plane and cuboid tranformations
uniform mat4 modelMatrix[6];

// models for whole cube transformation
uniform mat4 cubeModelMatrix;

void main() {
	vec4 worldPosition = cubeModelMatrix * modelMatrix[gl_InstanceID] * vec4(position, 1);
	vec4 viewPosition = projectionMatrix * viewMatrix * worldPosition;
	
	pass_texCoord = texCoord;

	instanceID = gl_InstanceID;

	gl_Position = viewPosition;
}