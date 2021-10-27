using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuListItem : MonoBehaviour, IMoveHandler
{
    public string Label
    {
        get => _buttonLabel.text;
        set => _buttonLabel.text = _valueLabel.text = value;
    }

    public string Value
    {
        get => _valueValue.text;
        set => _valueValue.text = value;
    }

    [BoxGroup("Button")] [SerializeField] [Label("Root")]
    private GameObject _buttonRoot;
    [BoxGroup("Button")] [SerializeField] [Label("Label")]
    private TextMeshProUGUI _buttonLabel;

    [BoxGroup("Value")] [SerializeField] [Label("Root")]
    private GameObject _valueRoot;
    [BoxGroup("Value")] [SerializeField] [Label("Label")]
    private TextMeshProUGUI _valueLabel;
    [BoxGroup("Value")] [SerializeField] [Label("Value")]
    private TextMeshProUGUI _valueValue;
    [BoxGroup("Value")] [SerializeField]
    private UnityEvent _leftMove;
    [BoxGroup("Value")] [SerializeField]
    private UnityEvent _rightMove;

    public enum ItemMode
    {
        Button,
        Value
    }

    [HideInInspector]
    [SerializeField]
    private ItemMode _mode;
    public ItemMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            Rebuild();
        }
    }

    private void Rebuild()
    {
        switch (_mode)
        {
            case ItemMode.Button:
                _buttonRoot.SetActive(true);
                _valueRoot.SetActive(false);
                break;
            
            case ItemMode.Value:
                _buttonRoot.SetActive(false);
                _valueRoot.SetActive(true);
                break;
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if (eventData.moveDir == MoveDirection.Left)
            _leftMove.Invoke();
        
        if (eventData.moveDir == MoveDirection.Right)
            _rightMove.Invoke();
    }
}