#version 330 core

flat in vec3 pass_Color;
out vec4 FragColor;

uniform vec3 color[54];

void main() {
	FragColor = vec4(pass_Color, 1);
}