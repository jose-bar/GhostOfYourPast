using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VignetteEffect : MonoBehaviour
{
    [Range(0, 1)]
    public float vignetteIntensity = 0.5f;
    [Range(0.1f, 3)]
    public float vignetteRoundness = 1f;
    [Range(0, 1)]
    public float vignetteSmoothness = 0.5f;

    public Color vignetteColor = new Color(0, 0, 0, 1);

    private Material vignetteMaterial;

    void Start()
    {
        // Make sure we have the vignette shader
        if (vignetteMaterial == null)
        {
            // Shutouts to CSE 167
            string shaderCode = @"
            Shader ""Custom/Vignette"" {
                Properties {
                    _MainTex (""Texture"", 2D) = ""white"" {}
                    _VignetteIntensity (""Vignette Intensity"", Range(0, 1)) = 0.5
                    _VignetteRoundness (""Vignette Roundness"", Range(0.1, 3)) = 1
                    _VignetteSmoothness (""Vignette Smoothness"", Range(0, 1)) = 0.5
                    _VignetteColor (""Vignette Color"", Color) = (0, 0, 0, 1)
                }
                
                SubShader {
                    Pass {
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #include ""UnityCG.cginc""
                        
                        struct appdata {
                            float4 vertex : POSITION;
                            float2 uv : TEXCOORD0;
                        };
                        
                        struct v2f {
                            float2 uv : TEXCOORD0;
                            float4 vertex : SV_POSITION;
                        };
                        
                        sampler2D _MainTex;
                        float _VignetteIntensity;
                        float _VignetteRoundness;
                        float _VignetteSmoothness;
                        float4 _VignetteColor;
                        
                        v2f vert(appdata v) {
                            v2f o;
                            o.vertex = UnityObjectToClipPos(v.vertex);
                            o.uv = v.uv;
                            return o;
                        }
                        
                        fixed4 frag(v2f i) : SV_Target {
                            fixed4 col = tex2D(_MainTex, i.uv);
                            
                            // Calculate vignette
                            float2 uv = i.uv - 0.5;
                            float vignette = 1.0 - pow(dot(uv, uv) * _VignetteRoundness, _VignetteSmoothness);
                            vignette = saturate(vignette);
                            vignette = pow(vignette, _VignetteIntensity * 3.0);
                            
                            // Apply vignette
                            return lerp(_VignetteColor, col, vignette);
                        }
                        ENDCG
                    }
                }
                FallBack ""Diffuse""
            }
            ";

            // Create material with this shader
            vignetteMaterial = new Material(Shader.Find("Custom/Vignette"));
            if (vignetteMaterial == null)
            {
                Debug.LogError("Could not find or create vignette shader!");
                enabled = false;
                return;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (vignetteMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Update material properties
        vignetteMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
        vignetteMaterial.SetFloat("_VignetteRoundness", vignetteRoundness);
        vignetteMaterial.SetFloat("_VignetteSmoothness", vignetteSmoothness);
        vignetteMaterial.SetColor("_VignetteColor", vignetteColor);

        // Apply effect
        Graphics.Blit(source, destination, vignetteMaterial);
    }
}
