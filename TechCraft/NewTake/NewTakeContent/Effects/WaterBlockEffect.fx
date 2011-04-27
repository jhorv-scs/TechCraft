float4x4 World;
float4x4 View;
float4x4 Projection;

float3 CameraPosition;

float FogNear = 250;
float FogFar = 300;
float4 FogColor = {0.5,0.5,0.5,1.0};
float3 SunColor;
float RippleTime = 0;
float timeOfDay;

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

struct VertexShaderInput
{
    float4 Position : POSITION0;	
	float2 TexCoords1 : TEXCOORD0;
	float SunLight : COLOR0;
    float3 LocalLight : COLOR1;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoords1 : TEXCOORD0;
    float3 CameraView : TEXCOORD1;
    float Distance : TEXCOORD2;
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

	output.Position.y += (0.2f * sin(RippleTime + input.Position.x + (input.Position.z*2) ))-0.2f;
	
    output.TexCoords1 = input.TexCoords1;

	float3 sColor = SunColor;

    if(timeOfDay <= 12)
	{
		sColor *= timeOfDay / 12;	
	}
	else
	{
		sColor *= (timeOfDay - 24) / -12;	
	}

	output.Color.rgb = (sColor * input.SunLight) + (input.LocalLight.rgb);
	output.Color.a = 1;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 texColor1 = tex2D(Texture1Sampler, input.TexCoords1);

    float fog = saturate((input.Distance - FogNear) / (FogNear-FogFar));    

    float4 color;
	color.rgb  = texColor1.rgb * input.Color.rgb;
	color.a = 0.5f; //texColor1.a;

    if(color.a == 0)
    {
        clip(-1);
    }

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
