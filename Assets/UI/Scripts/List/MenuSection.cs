using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class MenuSection : MonoBehaviour
{
    protected bool IsVisible => _visible;

    private bool _visible;
    private GameObject _focus;
    private GameObject _focusByDefault;
    private MenuSection _source;
    private GameObject _sourceFocus;
    private InputAction _cancel;
    private EventSystem _events;

    protected virtual void Awake()
    {
        _events = EventSystem.current;

        _visible = true;
        _cancel = new InputAction(binding: "*/{Cancel}");
        _cancel.Enable();
    }

    protected virtual void OnDisable()
    {
        if (IsChildFocused())
            _events.SetSelectedGameObject(null);
    }

    protected virtual void LateUpdate()
    {
        if (_visible && _source != null && _cancel.triggered && IsChildFocused())
        {
            Pop();
        }

        if (_visible)
        {
            if (_focus != null && _events.currentSelectedGameObject != _focus)
            {
                _events.SetSelectedGameObject(null);
                _events.SetSelectedGameObject(_focus);
            }

            _focus = null;
        }
    }

    protected void SetFocusByDefault(GameObject focus)
    {
        _focusByDefault = focus;
        _focus = focus;
    }

    protected bool IsChildFocused()
    {
        return _events.currentSelectedGameObject != null
            && _events.currentSelectedGameObject.transform.IsChildOf(transform);
    }

    public void Push(MenuSection source)
    {
        gameObject.SetActive(true);
        _visible = true;
        _focus = _focusByDefault;

        _sourceFocus = EventSystem.current.currentSelectedGameObject;
        _source = source;
        _source._visible = false;
    }

    public void Pop()
    {
        _visible = false;

        _source.gameObject.SetActive(true);
        _source._visible = true;
        _source._focus = _sourceFocus;
    }
}