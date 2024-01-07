shader_type canvas_item;

varying vec2 globalPosition;

uniform float pixelization = 16.0f;

uniform sampler2D gradientTexture : hint_default_white;
uniform sampler2D heightTexture : hint_default_white;
uniform sampler2D causticTexture : hint_default_white, repeat_enable;
uniform sampler2D causticNoiseTexture : hint_default_white, repeat_enable;
uniform sampler2D causticHighlightTexture : hint_default_white, repeat_enable;
uniform sampler2D causticFadeNoiseTexture : hint_default_white, repeat_enable;
uniform vec4 causticColor : source_color = vec4(0.455f, 0.773f, 0.765f, 1.0f);
uniform vec4 causticHighlightColor : source_color = vec4(0.741f, 0.894f, 0.898f, 1.0f);
uniform float causticScale = 12.0f;
uniform float causticSpeed = 1.0;
uniform float causticMovementAmount = 0.15f;
uniform float causticFaderMultiplier = 1.45f;

uniform vec4 specularColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
uniform sampler2D specularNoiseTexture : hint_default_white, repeat_enable;
uniform sampler2D specularMovementLeftNoiseTexture : hint_default_white, repeat_enable;
uniform sampler2D specularMovementRightNoiseTexture : hint_default_white, repeat_enable;
uniform float specularThreshold = 0.35f;
uniform float specularSpeed = 0.025f;
uniform float specularScale = 15.0f;

uniform vec4 foamColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
uniform sampler2D foamTexture : hint_default_white, repeat_enable;
uniform float foamIntensity = 0.2f;
uniform float foamScale = 15.0f;

uniform vec2 heightTextureGlobalPosition;
uniform float heightTextureSize = 176.0f;

void vertex() 
{
	globalPosition = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

// ------------------------------------------------------------------------------------
// Helper functions

// Blends two vec2's by subtracting them. Compare this to Photoshop blend mode "Subtract".
// Source:
// https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Blend-Node.html
vec2 blendSubtract_vec2(vec2 base, vec2 blend, float opacity)
{
	vec2 result = base - blend;
	return mix(base, result, opacity);
}

// Blends two floats by subtracting them. Compare this to Photoshop blend mode "Subtract".
// Source:
// https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Blend-Node.html
float blendSubtract_float(float base, float blend, float opacity)
{
	float result = base - blend;
	return mix(base, result, opacity);
}

// Blends two vec2's by overlaying them. Compare this to Photoshop blend mode "Overlay".
// Source:
// https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Blend-Node.html
float blendOverlay_float(float base, float blend, float opacity)
{
	float result1 = 1.0f - 2.0f * (1.0f - base) * (1.0f - blend);
    float result2 = 2.0f * base * blend;
    float zeroOrOne = step(0.5f, base);
    float res = result2 * zeroOrOne + (1.0 - zeroOrOne) * result1;
    return mix(base, res, opacity);
}

// Pixelizes the given coordinate.
vec2 pixelizeCoordinates(vec2 coordinates)
{
	return floor(coordinates * pixelization) / pixelization;
}

// ------------------------------------------------------------------------------------
// Shader layers

vec4 caustics(vec2 pixelizedCoordinates)
{
	vec4 causticNoise = texture(causticNoiseTexture, TIME * causticSpeed + pixelizedCoordinates);
	vec2 noiseCoordinates = blendSubtract_vec2(pixelizedCoordinates * causticScale, causticNoise.rg, causticMovementAmount);
	vec4 causticHighlight = texture(causticHighlightTexture, noiseCoordinates) * causticHighlightColor;
	vec4 caustic = texture(causticTexture, noiseCoordinates) * causticColor;
	vec4 interpolatedCaustics = mix(caustic, causticHighlight, causticHighlight.a);
	float fadeNoise = texture(causticFadeNoiseTexture, noiseCoordinates).r * causticFaderMultiplier;
	return vec4(interpolatedCaustics.r, interpolatedCaustics.g, interpolatedCaustics.b, clamp(interpolatedCaustics.a - fadeNoise, 0.0, 1.0));
}

vec4 specular(vec2 pixelizedCoordinates)
{
	vec2 scaledCoordinates = pixelizedCoordinates * specularScale;
	float specularNoise = texture(specularNoiseTexture, scaledCoordinates).r;
	float leftScrollingNoise = texture(specularMovementLeftNoiseTexture, scaledCoordinates + vec2(TIME * specularSpeed, 0.0f)).r;
	float rightScrollingNoise = texture(specularMovementRightNoiseTexture, scaledCoordinates + vec2(TIME * specularSpeed * -1.0f, 0.0f)).r;
	return step(specularThreshold, blendSubtract_float(blendOverlay_float(leftScrollingNoise, rightScrollingNoise, 1.0f), specularNoise, 1.0f)) * specularColor;
}

vec4 foam(vec2 pixelizedCoordinates, vec4 mainTexColor)
{
	vec4 colorizedFoam = texture(foamTexture, pixelizedCoordinates * foamScale) * foamColor;
	float intensity = clamp(mainTexColor.g * mainTexColor.a - foamIntensity, 0.0f, 1.0f);
	return vec4(colorizedFoam.r, colorizedFoam.g, colorizedFoam.b, colorizedFoam.a * intensity);
}


void fragment() 
{
	vec4 mainColor = texture(TEXTURE, UV);
	vec2 pixelizedCoordinates = pixelizeCoordinates(globalPosition / heightTextureSize);

	vec4 heightColor = texture(heightTexture, (globalPosition - heightTextureGlobalPosition) / heightTextureSize);
	vec4 gradientColor = texture(gradientTexture, vec2(heightColor.r, 0.5));
	gradientColor.a *= mainColor.a;
	
	vec4 finalCaustics = caustics(pixelizedCoordinates);
	finalCaustics.a *= mainColor.a;
	
	vec4 finalSpecular = specular(pixelizedCoordinates);
	vec4 finalFoam = foam(pixelizedCoordinates, mainColor);

	
	vec4 waterWithCausticLayer = mix(gradientColor, finalCaustics, finalCaustics.a);
	vec4 waterWithCausticAndSpecularLayer = mix(waterWithCausticLayer, finalSpecular, ceil(finalCaustics.a) * finalSpecular.a);
	vec4 waterWithCausticAndSpecularAndFoamLayer = mix(waterWithCausticAndSpecularLayer, finalFoam, finalFoam.a);
	
	
	
	vec4 blendColor = mix(gradientColor, finalCaustics, finalCaustics.a);
	
	COLOR = finalSpecular;
}

