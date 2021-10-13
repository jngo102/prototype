using System;
using UnityEngine;

[ExecuteAlways]
public class Squash2D : MonoBehaviour
{
    public float Angle
    {
        get => _squash.angle;
        set { _squash.angle = value; _dirty = true; }
    }

    public float ScaleAlong
    {
        get => _squash.scaleAlong;
        set { _squash.scaleAlong = value; _dirty = true; }
    }

    public float ScaleAcross
    {
        get => _squash.scaleAcross;
        set { _squash.scaleAcross = value; _dirty = true; }
    }

    [Serializable]
    private struct Squash
    {
        public float angle;
        public float scaleAlong;
        public float scaleAcross;

        public Squash(float angle, float scaleAlong, float scaleAcross)
        {
            this.angle = angle;
            this.scaleAcross = scaleAcross;
            this.scaleAlong = scaleAlong;
        }
    }

    [SerializeField] private Transform _squashRoot;
    [SerializeField] private Squash _squash = new Squash(0, 1, 1);

    private bool _dirty = true;

    private void OnValidate() => _dirty = true;
    private void Update()
    {
        if (_dirty)
        {
            _dirty = false;

            if (_squashRoot == null || _squashRoot.parent == null)
                return;

            _squashRoot.localRotation = Quaternion.Euler(0, 0, -_squash.angle);
            _squashRoot.parent.localRotation = Quaternion.Euler(0, 0, _squash.angle);
            _squashRoot.parent.localScale = new Vector3(_squash.scaleAlong, _squash.scaleAcross, 1);
        }
    }
}
