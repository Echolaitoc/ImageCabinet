//-------------------------------------------------------------------------------------------------------------------------------------------------
// compile: 
// .\fxc.exe /T ps_2_0 /Fo "D:\Files\Projects\ImageCabinet\UIHelper\RecolorEffect.ps" "D:\Files\Projects\ImageCabinet\UIHelper\RecolorEffect.fx"
//-------------------------------------------------------------------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);
float4 color : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 sampledColor = tex2D(implicitInputSampler, uv);
    sampledColor.rgb = color.rgb * sampledColor.a;
    return sampledColor;
}
