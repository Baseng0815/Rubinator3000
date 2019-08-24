﻿#version 330 core

flat in vec3 pass_Color;
in vec2 pass_texCoord;

out vec4 FragColor;

uniform vec3 color[54];

uniform sampler2D texture0;

void main() {
	FragColor = vec4(pass_Color, 1) * texture2D(texture0, pass_texCoord);
}