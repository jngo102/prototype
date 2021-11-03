using UnityEngine;

[ExecuteAlways]
public class ZSort : MonoBehaviour
{
    #if UNITY_EDITOR
    private void LateUpdate()
    {
        if (Application.isPlaying)
            return;

        var offset = (int)transform.localPosition.z;
        Arrange(transform, ref offset);
    }
    #endif

    private void Arrange(Transform transform, ref int offset)
    {
        var localPosition = transform.transform.localPosition;
        localPosition.z = offset;
        transform.localPosition = localPosition;

        var inner = 0;
        for (var i = transform.childCount-1; i >=0; i--)
            Arrange(transform.GetChild(i), ref inner);

        offset += 1;
        offset += inner;
    }
}
