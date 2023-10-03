#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 WorldViewProjection;
float2 Tiling;

texture Texture;
sampler2D textureSampler = sampler_state
{
    Texture = (Texture);
    MagFilter = Linear;
    MinFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
};


VertexShaderOutput BaseTilingVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, WorldViewProjection);

    // Propagate scaled Texture Coordinates
    output.TextureCoordinate = input.TextureCoordinate * Tiling;

    return output;
}

float4 BaseTilingPS(VertexShaderOutput input) : COLOR
{
    // Sample the texture using our scaled Texture Coordinates
    return tex2D(textureSampler, input.TextureCoordinate);
}

technique BaseTiling
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL BaseTilingVS();
        PixelShader = compile PS_SHADERMODEL BaseTilingPS();
    }
};
