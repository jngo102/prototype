using System.Collections;
using UnityEngine;
using EZCameraShake;

public class PlayerCameraController : MonoBehaviour
{
    public static PlayerCameraController Instance;

    private Camera _camera;
    private Player _player;
    private LevelBounds _bounds;
    private CameraShaker _shaker;
    private float _offset;

    private Vector2 _cameraPosition;
    private float _cameraDefaultHLerpSpeed = 5;
    private float _cameraDefaultVLerpSpeed = 8;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _shaker = CameraShaker.Instance;
    }

    public void Setup(Camera camera, Player player, LevelBounds bounds)
    {
        _camera = camera;
        _player = player;
        _bounds = bounds;

        // Instant movement to target
        _cameraPosition = _player.transform.position + new Vector3(0, 1.5f, 0);
        _shaker.RestPositionOffset = _camera.transform.position;
    }

    public void Shake(float magnitude = 1f)
    {
        if (_shaker == null)
            return;
        
        if (_shaker.ShakeInstances.Count > 0)
            return;

        _shaker.ShakeOnce(magnitude, 2f, .1f, .1f);
    }

    public void Instant()
    {
        if (_player != null)
        {
            _cameraPosition = _player.transform.position;
            _cameraPosition.y += 1.5f;
            _cameraPosition.y += _offset;
        }
    }

    public void LookAt(int offset)
    {
        _offset = offset * 5;
    }

    private void Update()
    {
        if (_camera == null)
            return;

        UpdateFollow();
        UpdateBoudns();
    }

    private void UpdateFollow()
    {
        if (_player == null)
            return;

        var targetPos = _player.transform.position;
        targetPos.y += 1.5f;
        targetPos.y += _offset;

        // Lerp
        _cameraPosition.x = Mathf.Lerp(_cameraPosition.x, targetPos.x, _cameraDefaultHLerpSpeed * Time.deltaTime);
        _cameraPosition.y = Mathf.Lerp(_cameraPosition.y, targetPos.y, _cameraDefaultVLerpSpeed * Time.deltaTime);
    }

    private void UpdateBoudns()
    {
        if (_bounds == null)
            return;

        var bPos = (Vector2)_bounds.transform.position;
        var bHalfSize = (Vector2)_bounds.Size / 2f;
        var cHalfSize = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);

        var min = bPos - bHalfSize + cHalfSize;
        var max = bPos + bHalfSize - cHalfSize;

        _cameraPosition = new Vector2(
            bHalfSize.x < cHalfSize.x ? bPos.x : Mathf.Clamp(_cameraPosition.x, min.x, max.x),
            bHalfSize.y < cHalfSize.y ? bPos.y : Mathf.Clamp(_cameraPosition.y, min.y, max.y)
        );
    }

    private void LateUpdate()
    {
        if (_camera == null)
            return;

        _camera.transform.position = new Vector3(
            _cameraPosition.x,
            _cameraPosition.y,
            _camera.transform.position.z
        );

        if (_shaker != null)
            _shaker.RestPositionOffset = _camera.transform.position;
    }
}