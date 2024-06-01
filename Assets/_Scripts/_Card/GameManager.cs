using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    private void Awake() => Inst = this;

    [SerializeField] private NotificationPanel notificationPanel;
    [SerializeField] private TMP_Text phaseTMP;
    [SerializeField] private TMP_Text displayActionTMP;
    [SerializeField] private TMP_Text moveAbleTMP;

    [SerializeField] private TMP_Text energyTMP;
    [SerializeField] private TMP_Text moveCostTMP;
    public bool onDisplayActions;
    public bool moveAble;

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        phaseTMP.text = "Phase: " + TurnManager.Inst.phase;
        energyTMP.text = TurnManager.Inst.Energy + " / " + TurnManager.Inst.maxEnergy;
        moveCostTMP.text = TurnManager.Inst.MoveCost + " / " + TurnManager.Inst.maxMoveCost;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            onDisplayActions = !onDisplayActions;
            displayActionTMP.text = "DisplayActions: " + onDisplayActions;

            if (onDisplayActions)
            {
                List<HexNode> selectedTiles = new();
                foreach (var unit in UnitManager.inst.enemies)
                {
                    if (!unit.card.canDisplay) return;
                    selectedTiles.AddRange(unit.card.SelectedArea);
                    GridManager.inst.AreaDisplay(AreaType.Attack, false, selectedTiles, unit);
                }
            }
            else
            {
                foreach (var unit in UnitManager.inst.enemies)
                {
                    GridManager.inst.RevertTiles(unit);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveAble = !moveAble;
            moveAbleTMP.text = "MoveAble: " + moveAble;
        }

        if (Input.GetMouseButtonDown(1))
        {
            UnitManager.inst.DeSelectUnit(UnitManager.sUnit);
            CameraManager.inst.SetOrthographicSize(false);
        }

#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey()
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
    }

    void StartGame()
    {
        TurnManager.Inst.GameSetUp();
    }

    public void Nodification(string message)
    {
        notificationPanel.Show(message);
    }
}
public static class YieldInstructionCache
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        WaitForSeconds wfs;
        if (!waitForSeconds.TryGetValue(seconds, out wfs))
            waitForSeconds.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }
}