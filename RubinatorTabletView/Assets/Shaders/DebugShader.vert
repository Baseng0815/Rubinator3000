#version 300 es

layout (location = 0) in vec3 aPosition;

void main() {
	gl_Position = vec4(aPosition, 0.0f);
}