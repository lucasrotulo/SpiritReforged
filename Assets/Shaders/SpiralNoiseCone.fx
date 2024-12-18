sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
matrix WorldViewProjection;
texture uTexture;
sampler textureSampler = sampler_state
{
    Texture = (uTexture);
    AddressU = wrap;
    AddressV = wrap;
};
float2 scroll;
float2 textureStretch;
float2 texExponentRange;
float finalIntensityMod;
float4 uColor;
float4 uColor2;
float finalExponent;
float taperRatio;
bool flipCoords;
float textureStrength;


struct VertexShaderInput
{
	float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;

	output.TextureCoordinates = input.TextureCoordinates;

    return output;
};

float GetAngle(float2 input, float2 centeredPos, float baseAngle)
{
    return atan2(input.y - centeredPos.y, input.x - centeredPos.x) + baseAngle;
}

const float fadeThreshold = 0.5f;
float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float xCoord = GetAngle(input.TextureCoordinates, float2(0.5f, 1), 1.57f);
    xCoord /= 3.14f / 4;
    xCoord *= lerp(3, 0.5f, pow(input.TextureCoordinates.y, 0.5f));
    xCoord += 0.5f;
    
    if (xCoord > 1 || xCoord < 0)
        return float4(0, 0, 0, 0);
    
    float noiseXCoord = ((xCoord - 0.5f) * textureStretch.x) + 0.5f;
    float2 noiseCoords = float2(noiseXCoord, (input.TextureCoordinates.y * textureStretch.y) + scroll.y);
    noiseCoords.x += lerp(0, scroll.x, pow(1 - input.TextureCoordinates.y, 2)) * textureStretch.y;
    noiseCoords.x %= 1;
    
    if (flipCoords)
    {
        float temp = noiseCoords.x;
        noiseCoords.x = noiseCoords.y;
        noiseCoords.y = temp;
    }
    
    float noiseExponent = lerp(texExponentRange.x, texExponentRange.y, input.TextureCoordinates.y);
    float strength = pow(tex2D(textureSampler, noiseCoords).r, noiseExponent) * textureStrength;
    float absXDist = 1 - (abs(xCoord - 0.5f) * 2);
    float circularXDist = sqrt(1 - pow(absXDist - 1, 2));
    strength += circularXDist;
    strength *= pow(circularXDist, 3);
    strength *= pow(input.TextureCoordinates.y, 0.5f);
    strength *= pow(1 - input.TextureCoordinates.y, 0.5f);
    
    float4 finalColor = lerp(uColor, uColor2, min(max(1 - strength, 0), 1));
    strength = pow(min(strength, 1), finalExponent);
    if (input.TextureCoordinates.y < fadeThreshold)
    {
        float progress = input.TextureCoordinates.y * (1 / fadeThreshold);
        strength *= pow(progress, 1.5f);
    }
    
    return input.Color * finalIntensityMod * strength * finalColor;
}

technique BasicColorDrawing
{
    pass PrimitiveTextureMap
	{
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
};