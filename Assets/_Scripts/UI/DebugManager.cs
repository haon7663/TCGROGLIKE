using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DebugManager : MonoBehaviour
{ 
    [SerializeField] private TMP_Text phaseTMP;
    
    
#if UNITY_EDITOR
    private void Update()
    {
        phaseTMP.text = "Phase: " + TurnManager.Inst.phase;
        InputCheatKey();
    }

    private void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            TurnManager.UseEnergy(TurnManager.Inst.Energy);

        if (Input.GetKeyDown(KeyCode.X))
            TurnManager.UseMoveCost(TurnManager.Inst.MoveCost);

        if (Input.GetKeyDown(KeyCode.C))
        {
            TurnManager.UseEnergy(TurnManager.Inst.Energy);
            TurnManager.UseMoveCost(TurnManager.Inst.MoveCost);
        }
        
        if (Input.GetKeyDown(KeyCode.Pause))
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
#endif
}
