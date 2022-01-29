using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_AntiGameUnit : MonoBehaviour
{
    public int Uid { get; private set; }
    public void SetUid(int uid) { Uid = uid; }
    public bool IsMoving { get; set; }
    void Start() { }
    void Update() { }
}
