using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatusSO", menuName = "Scriptable Object/StatusSO")]
public class StatusSO : ScriptableObject
{
    [Header("타입")]
    public StatusStackType stackType;
    public StatusCalculateType calculateType;

    [Header("표기")]
    public Sprite sprite;
    public string explain;
}

[Serializable]
public class StatusInfo
{
    public StatusSO data;
    public int stack;

    public StatusInfo(StatusSO data, int stack)
    {
        this.data = data;
        this.stack = stack;
    }
}
