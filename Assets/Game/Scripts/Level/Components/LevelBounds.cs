using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class LevelBounds : MonoBehaviour
{
    public Vector2Int Size => _size;

    [SerializeField] private Vector2Int _size;
    
    [HideInInspector] [SerializeField] private GameObject _container;
    [HideInInspector] [SerializeField] private GameObject _top;
    [HideInInspector] [SerializeField] private GameObject _right;
    [HideInInspector] [SerializeField] private GameObject _bottom;
    [HideInInspector] [SerializeField] private GameObject _left;

    private void Start()
    {
        AddBound(ref _top, "Top");
        AddBound(ref _right, "Right");
        AddBound(ref _bottom, "Bottom");
        AddBound(ref _left, "Left");

        OnValidate();
    }

    private void OnEnable() => _container.SetActive(true);
    private void OnDisable() => _container.SetActive(false);

    private void AddBound(ref GameObject gameObject, string name)
    {
        if (gameObject != null)
            return;

        if (_container == null)
        {
            _container = new GameObject("Bounds");
            _container.transform.SetParent(transform, false);
            _container.transform.SetSiblingIndex(0);
            _container.hideFlags = HideFlags.NotEditable;
        }

        gameObject = new GameObject(name);
        gameObject.transform.SetParent(_container.transform, false);
        gameObject.hideFlags = _container.hideFlags;
        gameObject.AddComponent<LevelGeometry>();
    }

    private void Update()
    {
        if (Application.isPlaying)
            return;

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    private void OnValidate()
    {
        _size.x = Mathf.Max(1, _size.x);
        _size.y = Mathf.Max(1, _size.y);

        if (_container != null)
        {
            _top.transform.localScale = _bottom.transform.localScale = new Vector3(_size.x + 1, 1, 1);
            _left.transform.localScale = _right.transform.localScale = new Vector3(1, _size.y + 1, 1);

            var half = (_size + Vector2.one) / 2f;
            var offset = 1/2f;

            _top.transform.localPosition = new Vector3(-offset, half.y, 0);
            _right.transform.localPosition = new Vector3(half.x, offset, 0);
            _bottom.transform.localPosition = new Vector3(offset, -half.y, 0);
            _left.transform.localPosition = new Vector3(-half.x, -offset, 0);
        }
    }
}
