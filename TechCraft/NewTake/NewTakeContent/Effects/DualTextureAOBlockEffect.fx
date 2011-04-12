float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float FogNear = 250;
float FogFar = 300;
float4 FogColor = {0.5,0.5,0.5,1.0};

Texture Texture1;
sampler Texture1Sampler = sampler_state
{
	texture = <Texture1>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

Texture Texture2;
sampler Texture2Sampler = sampler_state
{
	texture = <Texture2>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;	
	float2 TexCoords1 : TEXCOORD0;
	float2 TexCoords2 : TEXCOORD1;
	float AOWeight : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoords1 : TEXCOORD0;
	float2 TexCoords2 : TEXCOORD1;
    float3 CameraView : TEXCOORD2;
    float Distance : TEXCOORD3;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.CameraView = normalize(CameraPosition - worldPosition);
    output.Distance = length(CameraPosition - worldPosition);
    output.TexCoords1 = input.TexCoords1;
	output.TexCoords2 = input.TexCoords2;

	float3 aoColor = float3(1,1,1);
	output.Color.rgb = aoColor * input.AOWeight;
	output.Color.a = 1;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 texColor1 = tex2D(Texture1Sampler, input.TexCoords1);
	float4 texColor2 = tex2D(Texture2Sampler, input.TexCoords2);

	//float4 ambient = AmbientIntensity * AmbientColor;	    

    float fog = saturate((input.Distance - FogNear) / (FogNear-FogFar));    

    float4 color;
	color.rgb  =  (texColor1.rgb * texColor2.rgb) * input.Color.rgb;
	color.a = texColor1.a * texColor2.a;
    //color = texColor1;

    return lerp(FogColor, color ,fog);
}

technique BlockTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
