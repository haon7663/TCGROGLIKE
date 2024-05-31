using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSO", menuName = "Scriptable Object/PassiveSO")]
public abstract class PassiveSO : ScriptableObject
{
    public new string name;
    
    public virtual void Activate()
    {
        
    }
}