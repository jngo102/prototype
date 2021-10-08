using System;
using System.Linq;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    private Alignment _alignment = Alignment.BottomCenter;

    [Range(-1, 1)]
    [SerializeField]
    private float _amount;

    private Camera _camera;
    private Vector3 _levelCenter;
    private Vector3 _levelSize;
    private Vector3 _position;

    private void Start()
    {
        _camera = Camera.main;
        _levelCenter = GetComponentInParent<LevelBounds>()?.transform.position ?? Vector3.zero;
        _levelSize = GetComponentInParent<LevelBounds>()?.Size ?? Vector2.zero;
        _position = transform.position;
    }

    private void LateUpdate()
    {
        var center = GetInnerRectCenter(GetCameraSize(_camera), GetPivot(_alignment), _levelSize, _levelCenter);
        var offset = (_camera.transform.position - (Vector3)center) * _amount;
        offset.z = 0;

        transform.position = _position + offset;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
            var isSelected = UnityEditor.Selection.gameObjects.Any(x => x.transform.IsChildOf(transform));
            if (isSelected == false)
                return;
        #endif

        var camera = Camera.main;
        if (camera == null)
            return;
        
        var bounds = GetComponentInParent<LevelBounds>();
        if (bounds == null)
            return;

        var bRect = new Rect();
        bRect.size = bounds.Size;
        bRect.center = bounds.transform.position;

        var pRect = new Rect();
        pRect.size = bRect.size - (bRect.size - GetCameraSize(camera)) * _amount;
        pRect.center = GetInnerRectCenter(pRect.size, GetPivot(_alignment), bRect.size, bRect.center);

        var pRectPoints = new Vector3[] {
            new Vector3(pRect.xMin, pRect.yMin),
            new Vector3(pRect.xMin, pRect.yMax),
            new Vector3(pRect.xMax, pRect.yMax),
            new Vector3(pRect.xMax, pRect.yMin)
        };

        for (var i = 0; i < 4; i++)
        {
            var from = pRectPoints[i];
            var to = pRectPoints[(i+1) % pRectPoints.Length];
            Gizmos.DrawLine(from, to);
        }
    }

    private static Vector2 GetInnerRectCenter(Vector2 innerSize, Vector2 alignment, Vector2 outerSize, Vector2 outerCenter)
    {
        var innerOffset = (outerSize - innerSize) / 2;
        
        innerOffset.x *= alignment.x;
        innerOffset.y *= alignment.y;
        
        return outerCenter + innerOffset;
    }

    private static Vector2 GetCameraSize(Camera camera)
    {
        return new Vector2(
            2 * camera.orthographicSize * camera.aspect,
            2 * camera.orthographicSize
        );
    }

    private static Vector2 GetPivot(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.BottomLeft: return new Vector2(-1, -1);
            case Alignment.BottomCenter: return new Vector2(0, -1);
            case Alignment.BottomRight: return new Vector2(1, -1);

            case Alignment.MiddleLeft: return new Vector2(-1, 0);
            case Alignment.MiddleCenter: return new Vector2(0, 0);
            case Alignment.MiddleRight: return new Vector2(1, 0);

            case Alignment.TopLeft: return new Vector2(-1, 1);
            case Alignment.TopCenter: return new Vector2(0, 1);
            case Alignment.TopRight: return new Vector2(1, 1);
        }

        throw new Exception();
    }

    private enum Alignment
    {
        BottomLeft,
        BottomCenter,
        BottomRight,	
        
        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        TopLeft,
        TopCenter,
        TopRight
    }
}
