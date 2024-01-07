using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Object/UnitSO")]
public class UnitSO : ScriptableObject
{
    public string name;
    public int hp;
    public int arrangeCost;
    public int moveCost;

}
