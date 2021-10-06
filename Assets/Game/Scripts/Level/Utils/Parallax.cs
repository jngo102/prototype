using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Range(-1, 1)]
    public float _horizontal;
    [Range(-1, 1)]
    public float _vertical;

    private Camera _camera;
    private Vector3 _bounds;
    private Vector3 _start;

    private void Start()
    {
        _camera = Camera.main;
        _bounds = GetComponentInParent<LevelBounds>()?.transform.position ?? Vector3.zero;
        _start = transform.position;
    }

    private void LateUpdate()
    {
        var offset = (_camera.transform.position - _bounds);
        offset.x *= _horizontal;
        offset.y *= _vertical;
        offset.z = 0;
        transform.position = _start + offset;
    }

    private void OnDrawGizmos()
    {
        var bounds = GetComponentInParent<LevelBounds>();
        if (bounds == null)
            return;
        
        var camera = Camera.main;
        if (camera == null)
            return;

        var lvlExtents = bounds.Size / 2;
        var camExtents = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);

        var offsets = (lvlExtents - camExtents);
        offsets.x *= _horizontal;
        offsets.y *= _vertical;

        var extents = lvlExtents - offsets;
        var points = new [] { 
            new Vector3(-extents.x, -extents.y, 0),
            new Vector3(-extents.x, +extents.y, 0),
            new Vector3(+extents.x, +extents.y, 0),
            new Vector3(+extents.x, -extents.y, 0),
        };

        var center = bounds.transform.position;
        for (var i = 0; i < 4; i++)
        {
            var from = points[i];
            var to = points[(i+1)%points.Length];
            Gizmos.DrawLine(center + from, center + to);
        }
    }
}
