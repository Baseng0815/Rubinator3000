#version 330 core

in vec3 pass_fragPos;
in vec2 pass_texCoord;
in mat3 pass_TBNMat;

flat in int instanceID;

out vec4 FragColor;

uniform vec3 color[6];

// blend frame
uniform sampler2D texture0;

// bump map
uniform sampler2D texture1;


// lighting taken from LearnOpenGL.com/Lighting
struct DirLight {
	vec3 direction;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight {
	vec3 position;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

#define NUM_POINT_LIGHTS 3

const vec3 viewPos = vec3(0, 0, 2);

const DirLight directionalLight = DirLight(vec3(-.2f, -1.f, -.3f), vec3(.05f, .05f, .05f), 
	vec3(.4f, .4f, .4f), vec3(.5f, .5f, .5f));

const PointLight pointLights[NUM_POINT_LIGHTS] = PointLight[NUM_POINT_LIGHTS](
	PointLight(vec3(2.3f, -3.3f, -4.f), vec3(.05f, .05f, .05f), vec3(.8f, .8f, .8f), vec3(.5f, .5f, .5f)),
	PointLight(vec3(0.7f,  0.2f,  2.0f), vec3(.05f, .05f, .05f), vec3(.8f, .8f, .8f), vec3(.5f, .5f, .5f)),
	PointLight(vec3(0.0f,  0.0f, -3.0f), vec3(.05f, .05f, .05f), vec3(.8f, .8f, .8f), vec3(.5f, .5f, .5f))
);

vec3 calcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 calcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
	vec3 norm = texture(texture1, pass_texCoord).rgb;
	norm = normalize(norm * 2.f - 1.f);
	norm = normalize(pass_TBNMat * norm);

	vec3 viewDir = normalize(viewPos - pass_fragPos);

	vec3 result = calcDirLight(directionalLight, norm, viewDir);
	for (int i = 0; i < NUM_POINT_LIGHTS; i++)
		result += calcPointLight(pointLights[i], norm, pass_fragPos, viewDir);

	FragColor = vec4(result, 1.f);
}

vec3 calcDirLight(DirLight light, vec3 normal, vec3 viewDir) {
	vec3 lightDir = normalize(-light.direction);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.f);

    // combine results
	vec3 defaultColor = (vec4(color[instanceID], 1) * texture2D(texture0, pass_texCoord)).xyz;

    vec3 ambient = light.ambient * defaultColor;
    vec3 diffuse = light.diffuse * diff * defaultColor;
    vec3 specular = light.specular * spec * defaultColor;
    return (ambient + diffuse + specular);
}

vec3 calcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
	vec3 lightDir = normalize(light.position - fragPos);

    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);

    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.f);

    // combine results
	vec3 defaultColor = (vec4(color[instanceID], 1) * texture2D(texture0, pass_texCoord)).xyz;

    vec3 ambient = light.ambient * defaultColor;
    vec3 diffuse = light.diffuse * diff * defaultColor;
    vec3 specular = light.specular * spec * defaultColor;
    return (ambient + diffuse + specular);
}