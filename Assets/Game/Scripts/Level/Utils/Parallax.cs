using System;
using System.Drawing;
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
        var k = GetPivot(_alignment);
        var center = GetCameraCenter(_camera, k, _levelSize, _levelCenter);
        var offset = (_camera.transform.position - (Vector3)center) * _amount;
        offset.z = 0;

        transform.position = _position + offset;
    }

    private void OnDrawGizmosSelected()
    {
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
        pRect.center = GetCameraCenter(camera, GetPivot(_alignment), bRect.size, bRect.center);

        var visible = Intersect(bRect, pRect);
        var visiblePoints = new Vector3[] {
            new Vector3(visible.xMin, visible.yMin),
            new Vector3(visible.xMin, visible.yMax),
            new Vector3(visible.xMax, visible.yMax),
            new Vector3(visible.xMax, visible.yMin)
        };

        for (var i = 0; i < 4; i++)
        {
            var from = visiblePoints[i];
            var to = visiblePoints[(i+1) % visiblePoints.Length];
            Gizmos.DrawLine(from, to);
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var center = GetCameraCenter(camera, new Vector2(x, y), bounds.Size, bounds.transform.position);
                Gizmos.DrawSphere(center, 0.3f);
            }
        }
    }

    private static Rect Intersect(Rect rect1, Rect rect2)
    {
        var r1 = new RectangleF(rect1.x, rect1.y, rect1.width, rect1.height);
        var r2 = new RectangleF(rect2.x, rect2.y, rect2.width, rect2.height);
        var intersect = RectangleF.Intersect(r1, r2);
        var result = new Rect(intersect.X, intersect.Y, intersect.Width, intersect.Height);

        return result;
    }

    private static Vector2 GetCameraCenter(Camera camera, Vector2 alignment, Vector2 levelBounds, Vector2 levelCenter)
    {
        var offset = levelBounds/2 - GetCameraSize(camera)/2;

        offset.x *= alignment.x;
        offset.y *= alignment.y;

        return levelCenter + offset;
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