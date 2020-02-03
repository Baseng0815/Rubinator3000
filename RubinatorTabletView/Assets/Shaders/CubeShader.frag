#version 300 es
precision highp float;

in vec2 pass_texCoord;

flat in int instanceID;

out vec4 FragColor;

uniform vec3 color[6];

// blend frame
uniform sampler2D texture0;

void main()
{
	FragColor = vec4(color[instanceID] * texture(texture0, pass_texCoord).xyz, 1);
}