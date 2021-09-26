using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class ColorMatrix : MonoBehaviour
{
    private static string SHADER_NAME = "Hidden/ColorMatrix";
    private static Material sMaterial;
    private static Material sMaterialETC1;

    private Matrix4x5 matrix4x5 = new Matrix4x5();
    private Matrix4x4 matrix4x4 = new Matrix4x4();
    private Vector4 offset = new Vector4();

    private bool dirty;
    private List<MaterialOwner> renderers = new List<MaterialOwner>();

    private bool hasAlphaTextures = false;

    [SerializeField]
    [Range(-1, +1)]
    private float brightness;
    public float Brightness
    {
        get { return brightness; }
        set
        {
            dirty = true;
            brightness = value;
        }
    }

    [SerializeField]
    [Range(-1, +1)]
    private float saturation;
    public float Saturation
    {
        get { return saturation; }
        set
        {
            dirty = true;
            saturation = value;
        }
    }

    [SerializeField]
    [Range(-1, +1)]
    private float hue;
    public float Hue
    {
        get { return hue; }
        set
        {
            dirty = true;
            hue = value;
        }
    }

    //
    // Setups
    //
    public void Start()
    {
        Reset();
    }

    public void Reset()
    {
        if (sMaterial == null)
        {
            sMaterial = new Material(Shader.Find(SHADER_NAME));
            sMaterial.name += " (Shared)";
        }
        if (sMaterialETC1 == null)
        {
            sMaterialETC1 = new Material(Shader.Find(SHADER_NAME));
            sMaterialETC1.SetFloat("_EnableETC1", 1);
            sMaterialETC1.name += " ETC1 (Shared)";
        }

        dirty = true;
        renderers.Clear();

        foreach (var renderer in GetComponents<Renderer>())
        {
            renderers.Add(new MaterialOwner(renderer, new Material(Shader.Find(SHADER_NAME)), sMaterial, sMaterialETC1));
        }

        var image = GetComponent<Image>();
        if (image != null)
        {
            renderers.Add(new MaterialOwner(image, new Material(Shader.Find(SHADER_NAME)), sMaterial, sMaterialETC1));
            hasAlphaTextures = image.sprite.associatedAlphaSplitTexture != null;
        }
    }

    //
    // Invalidations
    //

    public void OnValidate() { dirty = true; }
    public void OnDidApplyAnimationProperties() { dirty = true; }

    //
    // Update
    //

    public void LateUpdate()
    {
        if (dirty)
        {
            dirty = false;
            UpdateMatrix();
            UpdateMatrials();
        }
    }

    private void UpdateMatrix()
    {
        matrix4x5.Identity();

        // Brightness
        matrix4x5.Concat(
            1, 0, 0, 0, Brightness,
            0, 1, 0, 0, Brightness,
            0, 0, 1, 0, Brightness,
            0, 0, 0, 1, 0
        );

        // Saturation
        var sat = saturation + 1;
        var invSat = 1 - sat;
        var invLumR = invSat * LUMA_R;
        var invLumG = invSat * LUMA_G;
        var invLumB = invSat * LUMA_B;

        matrix4x5.Concat(
            invLumR + sat, invLumG, invLumB, 0, 0,
            invLumR, invLumG + sat, invLumB, 0, 0,
            invLumR, invLumG, invLumB + sat, 0, 0,
            0, 0, 0, 1, 0
        );

        // Hue
        var hueAngle = hue * Mathf.PI;
        var cos = Mathf.Cos(hueAngle);
        var sin = Mathf.Sin(hueAngle);

        matrix4x5.Concat(
            LUMA_R + cos * (1 - LUMA_R) + sin * -LUMA_R, LUMA_G + cos * -LUMA_G + sin * -LUMA_G, LUMA_B + cos * -LUMA_B + sin * (1 - LUMA_B), 0, 0,
            LUMA_R + cos * -LUMA_R + sin * 0.143f, LUMA_G + cos * (1 - LUMA_G) + sin * 0.14f, LUMA_B + cos * -LUMA_B + sin * -0.283f, 0, 0,
            LUMA_R + cos * -LUMA_R + sin * -(1 - LUMA_R), LUMA_G + cos * -LUMA_G + sin * LUMA_G, LUMA_B + cos * (1 - LUMA_B) + sin * LUMA_B, 0, 0,
            0, 0, 0, 1, 0);

        // Output
        matrix4x5.CopyTo(ref matrix4x4);
        matrix4x5.CopyTo(ref offset);
    }

    private void UpdateMatrials()
    {
        var isIdentity = brightness == 0 && saturation == 0 && hue == 0;

        foreach (var owner in renderers)
        {
            if (isIdentity) owner.UseSharedMaterial();
            else
            {
                owner.UseInstanceMaterial();
                owner.Material.SetMatrix("_ColorMatrix", matrix4x4);
                owner.Material.SetVector("_ColorOffset", offset);
                owner.Material.SetFloat("_EnableETC1", hasAlphaTextures ? 1 : 0);
            }
        }
    }

    //
    // Helpers
    //

    private const float LUMA_R = 0.299f;
    private const float LUMA_G = 0.587f;
    private const float LUMA_B = 0.114f;

    private class Matrix4x5
    {
        private static float[] temp = new float[20];
        private static Matrix4x5 buffer = new Matrix4x5(false);

        private float[] m;

        public Matrix4x5(bool identity = true)
        {
            m = new float[20];

            if (identity)
                Identity();
        }

        public void Identity()
        {
            Set(1, 0, 0, 0, 0,
                0, 1, 0, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 0, 1, 0);
        }

        public void Set(float m00, float m01, float m02, float m03, float m04,
                        float m10, float m11, float m12, float m13, float m14,
                        float m20, float m21, float m22, float m23, float m24,
                        float m30, float m31, float m32, float m33, float m34)
        {
            m[00] = m00; m[01] = m01; m[02] = m02; m[03] = m03; m[04] = m04;
            m[05] = m10; m[06] = m11; m[07] = m12; m[08] = m13; m[09] = m14;
            m[10] = m20; m[11] = m21; m[12] = m22; m[13] = m23; m[14] = m24;
            m[15] = m30; m[16] = m31; m[17] = m32; m[18] = m33; m[19] = m34;
        }

        public void Concat(float m00, float m01, float m02, float m03, float m04,
                           float m10, float m11, float m12, float m13, float m14,
                           float m20, float m21, float m22, float m23, float m24,
                           float m30, float m31, float m32, float m33, float m34)
        {
            buffer.Set(m00, m01, m02, m03, m04,
                       m10, m11, m12, m13, m14,
                       m20, m21, m22, m23, m24,
                       m30, m31, m32, m33, m34);

            Concat(ref buffer);
        }

        public void Concat(ref Matrix4x5 matrix)
        {
            int i = 0;

            for (var y = 0; y < 4; ++y)
            {
                for (var x = 0; x < 5; ++x)
                {
                    temp[i + x] = matrix.m[i] * m[x] +
                                matrix.m[i + 1] * m[x + 5] +
                                matrix.m[i + 2] * m[x + 10] +
                                matrix.m[i + 3] * m[x + 15] +
                                (x == 4 ? matrix.m[i + 4] : 0);
                }

                i += 5;
            }

            temp.CopyTo(m, 0);
        }

        public void CopyTo(ref Matrix4x4 matrix)
        {
            matrix.m00 = m[0];
            matrix.m01 = m[1];
            matrix.m02 = m[2];
            matrix.m03 = m[3];

            matrix.m10 = m[5];
            matrix.m11 = m[6];
            matrix.m12 = m[7];
            matrix.m13 = m[8];

            matrix.m20 = m[10];
            matrix.m21 = m[11];
            matrix.m22 = m[12];
            matrix.m23 = m[13];

            matrix.m30 = m[15];
            matrix.m31 = m[16];
            matrix.m32 = m[17];
            matrix.m33 = m[18];
        }

        public void CopyTo(ref Vector4 offset)
        {
            offset.x = m[4];
            offset.y = m[9];
            offset.z = m[14];
            offset.w = m[19];
        }
    }

    private class MaterialOwner
    {
        private Material sharedMaterial;
        private Material sharedMaterialETC1;
        private Material material;
        private Renderer renderer;
        private Image image;

        public MaterialOwner(Renderer renderer, Material material, Material sharedMaterial, Material sharedMaterialETC1)
        {
            this.material = material;
            this.renderer = renderer;
            this.sharedMaterial = sharedMaterial;
            this.sharedMaterialETC1 = sharedMaterialETC1;
        }

        public MaterialOwner(Image image, Material material, Material sharedMaterial, Material sharedMaterialETC1)
        {
            this.material = material;
            this.image = image;
            this.sharedMaterial = sharedMaterial;
            this.sharedMaterialETC1 = sharedMaterialETC1;
        }

        public void UseSharedMaterial()
        {
            if (image && image.sprite != null)
            {
                image.material = image.sprite.associatedAlphaSplitTexture != null ? sharedMaterialETC1 : sharedMaterial;
            }

            if (renderer)
                renderer.sharedMaterial = sharedMaterial;
        }

        public void UseInstanceMaterial()
        {
            if (image)
                image.material = material;

            if (renderer)
                renderer.material = material;
        }

        public Material Material => material;
    }
}