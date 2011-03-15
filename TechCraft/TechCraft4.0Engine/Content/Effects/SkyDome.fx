//------- Constants --------
float4x4 View;
float4x4 Projection;
float4x4 World;

//------- Texture Samplers --------
Texture SkyTexture;

sampler TextureSampler = sampler_state { texture = <SkyTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

//------- Technique: SkyDome --------
struct SDVertexToPixel
{  
  float4 Position   : POSITION;
  float2 TextureCoords : TEXCOORD0;
  float4 ObjectPosition : TEXCOORD1;
};

struct SDPixelToFrame
{
  float4 Color : COLOR0;
};

SDVertexToPixel SkyDome_VS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{  
  SDVertexToPixel Output = (SDVertexToPixel)0;
  float4x4 preViewProjection = mul (View, Projection);
  float4x4 preWorldViewProjection = mul (World, preViewProjection);
  
  Output.Position = mul(inPos, preWorldViewProjection);
  Output.TextureCoords = inTexCoords;
  Output.ObjectPosition = inPos;
  
  return Output;  
}

SDPixelToFrame SkyDome_PS(SDVertexToPixel PSIn)
{
  SDPixelToFrame Output = (SDPixelToFrame)0;    

  float4 topColor = float4(0.3f, 0.3f, 0.8f, 1);  
  float4 bottomColor = 1;  
  
  float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.4f));
  float4 cloudValue = tex2D(TextureSampler, PSIn.TextureCoords).r;
  
  Output.Color = lerp(baseColor,1, cloudValue);    

  return Output;
}

technique SkyDome
{
  pass Pass0
  {
    VertexShader = compile vs_2_0 SkyDome_VS();
    PixelShader = compile ps_2_0 SkyDome_PS();
  }
}