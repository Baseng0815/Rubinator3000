#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 normal;
layout (location = 2) in vec2 texCoord;

// models for each tile
uniform mat4 modelMatrix[54];
uniform vec3 color[54];

flat out vec3 pass_Color;

void main() {
	vec4 worldPosition = modelMatrix[gl_InstanceID] * vec4(position, 1);

	gl_Position = worldPosition;

	pass_Color = color[gl_InstanceID];
}