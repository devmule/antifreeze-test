using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_AntiGameUnit : MonoBehaviour
{

    [SerializeField] public UnityEvent OnUnitSelected;
    [SerializeField] public UnityEvent OnUnitDiselected;
    [SerializeField] public UnityEvent OnUnitBeginMove;
    [SerializeField] public UnityEvent OnUnitEndMove;

    private bool _isMoving = false;
    private bool _selected = false;

    public int Uid { get; private set; }
    public void SetUid(int uid) { Uid = uid; }

    public void SetMoving(bool isMoving) 
    {
        if (_isMoving == isMoving) return;
        _isMoving = isMoving;
        if (isMoving) { OnUnitBeginMove?.Invoke(); }
        else { OnUnitEndMove?.Invoke(); }
    }
    public void SetSelected(bool isSelected) 
    {
        if (_selected == isSelected) return;
        _selected = isSelected;
        if (isSelected) { OnUnitSelected?.Invoke(); }
        else { OnUnitDiselected?.Invoke(); }
    }

    void Start() { }
    void Update() { }
}
