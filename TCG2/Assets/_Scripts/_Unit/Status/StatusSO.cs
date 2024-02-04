using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusSO", menuName = "Scriptable Object/StatusSO")]
public class StatusSO : ScriptableObject
{
    [Header("Ÿ��")]
    public StatusStackType stackType;
    public StatusCalculateType calculateType;

    [Header("ǥ��")]
    public Sprite sprite;
    public string explain;
}