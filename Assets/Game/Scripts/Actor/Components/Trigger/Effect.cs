using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public interface IEffect
{
    void Play();
}

[Serializable, TypePopupLabel("General/List")]
public class ComplexEffect : IEffect
{
    [SerializeReference, TypePopup]
    private List<IEffect> _items;

    public void Play()
    {
        for (var i = 0; i < _items.Count; i++)
            if (_items[i] != null)
                _items[i].Play();
    }
}

[Serializable, TypePopupLabel("General/UnityEvent")]
public class UnityEventEffect : IEffect
{
    [SerializeField]
    private UnityEvent _event;

    public void Play() => _event.Invoke();
}

[Serializable, TypePopupLabel("GameObject/SetActive")]
public class SetActiveEffect : IEffect
{
    [SerializeField]
    private GameObject _gameObject;

    [SerializeField]
    private bool _value;

    public void Play() { _gameObject.SetActive(_value); }
}

[Serializable, TypePopupLabel("GameObject/Instantiate")]
public class InstantiateEffect : IEffect
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private Transform _parent;

    public void Play() { GameObject.Instantiate(_prefab, _parent); }
}

[Serializable, TypePopupLabel("Animator/SetTrigger")]
public class SetTriggerEffect : IEffect
{
    [SerializeField]
    public Animator _animator;

    [AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Trigger)]
    [SerializeField]
    public string _trigger;

    public void Play() { _animator.SetTrigger(_trigger); }
}

[Serializable, TypePopupLabel("Animator/SetBool")]
public class SetBoolEffect : IEffect
{
    [SerializeField]
    private Animator _animator;

    [AnimatorParam(nameof(_animator), AnimatorControllerParameterType.Bool)]
    [SerializeField]
    private string _parameter;

    [SerializeField]
    private bool _value;

    public void Play() { _animator.SetBool(_parameter, _value); }
}

[Serializable, TypePopupLabel("Camera/Shake")]
public class CameraShakeEffect : IEffect
{
    [SerializeField, Range(0, 1)]
    private float _magnitude = 0.25f;

    [SerializeField, Range(0, 10)]
    private float _roughness = 2f;

    [SerializeField, Range(0, 1)]
    private float _fadeInTime = 0.1f;

    [SerializeField, Range(0, 1)]
    private float _fadeOutTime = 0.1f;

    public void Play()
    {
        PlayerCameraController.Instance.Shake(
            _magnitude,
            _roughness,
            _fadeInTime,
            _fadeOutTime
        );
    }
}

// [Serializable]
// public class ColorMatrixEffect : IEffect
// {
//     public ColorMatrix ColorMatrix;
//     public AnimationCurve Brightness = AnimationCurve.Linear(0, 0, 1, 0); 

//     public void Play() { }
// }

// [Serializable]
// public class SoundEffect : IEffect
// {
//     public AudioClip Clip; 

//     public void Play() { }
// }