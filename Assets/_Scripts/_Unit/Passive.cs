using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Passive", menuName = "Scriptable Object/Passive")]
public abstract class Passive : ScriptableObject
{
    public new string name;
    
    public virtual void Activate()
    {
        
    }
}