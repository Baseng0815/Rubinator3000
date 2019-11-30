#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 tangent;
layout (location = 3) in vec3 bitangent;
layout (location = 4) in vec2 texCoord;

out vec2 pass_texCoord;

uniform mat4 modelMatrix;

void main() {
	vec4 worldPosition = modelMatrix * vec4(position, 1);

	gl_Position = worldPosition;
}
