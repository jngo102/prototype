using UnityEngine;

public abstract class CameraPipelinePostEffect : ScriptableObject
{
    public abstract void OnRenderImage(RenderTexture source, RenderTexture destination);



    protected Material CreateMaterial (Shader s, Material m2Create)
    {
        if (!s)
        {
            Debug.Log ("Missing shader in " + ToString ());
            return null;
        }

        if (m2Create && (m2Create.shader == s) && (s.isSupported))
            return m2Create;

        if (!s.isSupported)
        {
            return null;
        }
        else
        {
            m2Create = new Material (s);
            m2Create.hideFlags = HideFlags.DontSave;
            if (m2Create)
                return m2Create;
            else return null;
        }
    }
}