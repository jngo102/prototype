using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LevelGeometry : Actor
{
    //
    // Colors
    //

    public static Color Solid => Parse("#83858C");
    public static Color Gate => Parse("#F3A738");
    public static Color Spike => Parse("#D81159");
    public static Color Fragile => Parse("#DDC9B4");
    public static Color Water => Parse("#52489c");

    private static Color Parse(string htmlString)
    {
        var result = ColorUtility.TryParseHtmlString(htmlString, out Color color);
        color.a = 0.8f;
        return result ? color : Color.magenta;
    }

    //
    // Data
    //
    [HideInInspector] [SerializeField] private Sprite _sprite;
    [HideInInspector] [SerializeField] private BoxCollider2D _collider;
    [HideInInspector] [SerializeField] private SpriteRenderer _renderer;

    public enum GeometryType { Solid, Spike, Fragile, Water }
    public GeometryType Type => _type;
    public Bounds Bounds => _collider.bounds;

    [SerializeField] private GeometryType _type;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        _renderer.hideFlags = HideFlags.NotEditable;
        _renderer.sprite = _sprite;
        _renderer.color = LevelGeometry.Solid;

        _collider.hideFlags = HideFlags.NotEditable;
        _collider.isTrigger = false;
        
        if (Type != GeometryType.Spike)
            gameObject.layer = 3;

        OnValidate();
    }

    private void Update()
    {
        if (Application.isPlaying)
            return;

        Round(transform);
    }

    private void OnValidate()
    {
        if (_renderer == null)
            return;

        switch (_type)
        {
            case GeometryType.Solid: _renderer.color = LevelGeometry.Solid; break;
            case GeometryType.Spike: _renderer.color = LevelGeometry.Spike; break;
            case GeometryType.Fragile: _renderer.color = LevelGeometry.Fragile; break;
            case GeometryType.Water: _renderer.color = LevelGeometry.Water; break;
        }
    }

    public static void Round(Transform transform)
    {
        var sx = Round(transform.localScale.x);
        var sy = Round(transform.localScale.y);

        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(sx, sy, 1);
        transform.localPosition = new Vector3(
            Round(transform.localPosition.x - sx/2f) + sx/2f,
            Round(transform.localPosition.y - sy/2f) + sy/2f,
            0
        );
    }

    private static float Round(float value, float k = 4f)
    {
        return Mathf.RoundToInt(value * k) / k;
    }
}
