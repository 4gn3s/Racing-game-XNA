float4x4 View;
float4x4 Projection;

texture ParticleTexture;
sampler2D texSampler = sampler_state {
	texture = <ParticleTexture>;
};

bool AlphaTest = true;
bool AlphaTestGreater = true;
float AlphaTestValue = 0.95f;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position, mul(View, Projection));
	output.UV = input.UV;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(texSampler, input.UV);

	if (AlphaTest)
		clip((color.a - AlphaTestValue) * (AlphaTestGreater ? 1 : -1));

	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
