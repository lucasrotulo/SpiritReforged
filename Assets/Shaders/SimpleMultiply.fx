sampler uImage0 : register(s0);

texture cTexture;
sampler c
{
    Texture = (cTexture);
};

float2 offset;

float4 MainPS(float2 coords : TEXCOORD0, float4 ocolor : COLOR0) : COLOR0
{
    float2 coordinates = coords + offset;
    
    float4 texColor = tex2D(uImage0, coords);
    float4 saColor = tex2D(c, coordinates);

    return ocolor * texColor * saColor;
}

technique BasicColorDrawing
{
    pass MainPS
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};