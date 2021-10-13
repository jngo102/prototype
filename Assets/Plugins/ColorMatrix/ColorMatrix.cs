using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class ColorMatrix : MonoBehaviour
{
    private static string SHADER_NAME = "Hidden/ColorMatrix";
    private static Matrix4x5 sMatrix4x5 = new Matrix4x5();

    private static Dictionary<Vector3, Material> sMaterialsValues = new Dictionary<Vector3, Material>();
    private static Dictionary<Material, Vector3> sMaterialsKeys = new Dictionary<Material, Vector3>();
    private static Dictionary<Vector3, int> sMaterialsCounter = new Dictionary<Vector3, int>();
    private static Stack<Material> sMaterialsPool = new Stack<Material>();

    private static Material Get(float b, float s, float h)
    {
        var key = new Vector3(b, s, h);

        var hasMaterial = sMaterialsValues.ContainsKey(key) && sMaterialsValues[key] != null;
        if (hasMaterial == false)
        {
            // In case we switch playmode all materials are destroyed thus pool is invalid
            if (sMaterialsPool.Count > 0 && sMaterialsPool.Peek() == null)
                sMaterialsPool.Clear();

            var material = sMaterialsPool.Count > 0
                ? sMaterialsPool.Pop()
                : new Material(Shader.Find(SHADER_NAME));
            
            material.hideFlags = HideFlags.HideAndDontSave;
            
            var matrix = new Matrix4x4();
            var offset = new Vector4();
            CalcMatrix(b, s, h, ref matrix, ref offset);

            material.SetMatrix("_ColorMatrix", matrix);
            material.SetVector("_ColorOffset", offset);

            sMaterialsKeys[material] = key;
            sMaterialsValues[key] = material;
            sMaterialsCounter[key] = 0;
        }

        sMaterialsCounter[key]++;

        return sMaterialsValues[key];
    }

    private static void Release(Material material)
    {
        var key = sMaterialsKeys[material];
        var coutner = --sMaterialsCounter[key];

        if (coutner == 0)
        {
            sMaterialsKeys.Remove(material);
            sMaterialsValues.Remove(key);
            sMaterialsCounter.Remove(key);
            sMaterialsPool.Push(material);
        }
    }

    private bool dirty;
    private List<MaterialRenderer> renderers = new List<MaterialRenderer>();

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

    private void OnEnable()
    {
        var matrix = transform.parent != null ? transform.parent.GetComponentInParent<ColorMatrix>() : null;
        if (matrix != null)
        {
            enabled = false;
            Debug.LogWarning($"{nameof(ColorMatrix)} already exists on '{matrix.name}'");
        }
        else
        {
            dirty = true;

            renderers.Clear();

            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderers.Add(new MaterialRenderer(renderer));

            foreach (var image in GetComponentsInChildren<Image>())
                renderers.Add(new MaterialRenderer(image));
        }
    }

    private void OnDisable()
    {
        foreach (var renderer in renderers)
            renderer.Destroy();
        
        renderers.Clear();
    }

    //
    // Invalidations
    //

    public void Reset() { dirty = true; }
    public void OnValidate() { dirty = true; }
    public void OnDidApplyAnimationProperties() { dirty = true; }

    //
    // Update
    //

    public void Update()
    {
        if (dirty)
        {
            dirty = false;

            foreach (var renderer in renderers)
                renderer.Set(Brightness, Saturation, Hue);
        }
    }

    //
    // Helpers
    //
    private const float LUMA_R = 0.299f;
    private const float LUMA_G = 0.587f;
    private const float LUMA_B = 0.114f;

    private static void CalcMatrix(float b, float s, float h, ref Matrix4x4 matrix4x4, ref Vector4 offset)
    {
        sMatrix4x5.Identity();

        // Brightness
        sMatrix4x5.Concat(
            1, 0, 0, 0, b,
            0, 1, 0, 0, b,
            0, 0, 1, 0, b,
            0, 0, 0, 1, 0
        );

        // Saturation
        var sat = s + 1;
        var invSat = 1 - sat;
        var invLumR = invSat * LUMA_R;
        var invLumG = invSat * LUMA_G;
        var invLumB = invSat * LUMA_B;

        sMatrix4x5.Concat(
            invLumR + sat, invLumG, invLumB, 0, 0,
            invLumR, invLumG + sat, invLumB, 0, 0,
            invLumR, invLumG, invLumB + sat, 0, 0,
            0, 0, 0, 1, 0
        );

        // Hue
        var hueAngle = h * Mathf.PI;
        var cos = Mathf.Cos(hueAngle);
        var sin = Mathf.Sin(hueAngle);

        sMatrix4x5.Concat(
            LUMA_R + cos * (1 - LUMA_R) + sin * -LUMA_R, LUMA_G + cos * -LUMA_G + sin * -LUMA_G, LUMA_B + cos * -LUMA_B + sin * (1 - LUMA_B), 0, 0,
            LUMA_R + cos * -LUMA_R + sin * 0.143f, LUMA_G + cos * (1 - LUMA_G) + sin * 0.14f, LUMA_B + cos * -LUMA_B + sin * -0.283f, 0, 0,
            LUMA_R + cos * -LUMA_R + sin * -(1 - LUMA_R), LUMA_G + cos * -LUMA_G + sin * LUMA_G, LUMA_B + cos * (1 - LUMA_B) + sin * LUMA_B, 0, 0,
            0, 0, 0, 1, 0);

        // Output
        sMatrix4x5.CopyTo(ref matrix4x4);
        sMatrix4x5.CopyTo(ref offset);
    }

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

    private class MaterialRenderer
    {
        private Material material;
        private Renderer renderer;
        private Image image;

        public MaterialRenderer(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public MaterialRenderer(Image image)
        {
            this.image = image;
        }

        public void Set(float b, float s, float h)
        {
            if (material != null)
                Release(material);

            material = Get(b, s, h);

            if (image)
                image.material = material;

            if (renderer)
                renderer.material = material;
        }

        public void Destroy()
        {
            if (material != null)
                Release(material);
        }
    }
}